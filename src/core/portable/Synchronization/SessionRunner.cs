﻿using Fuxion.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Fuxion.Synchronization
{
    internal class SessionRunner
    {
        public SessionRunner(Session definition, IPrinter printer)
        {
            this.printer = printer;
            this.Session = definition;
        }
        IPrinter printer;
        internal Session Session { get; set; }
        ICollection<WorkRunner> works = new List<WorkRunner>();
        public Guid Id { get { return Session.Id; } }
        public Task<SessionPreview> PreviewAsync(bool includeNoneActionItems, IPrinter printer)
        {
            return printer.IndentAsync($"Previewing synchronization session '{Session.Name}' {(Session.MakePreviewInParallel ? "in parallel" : "sequentially")}",
                async () =>
                {
                    var res = new SessionPreview(Session.Id);
                    works = Session.Works.Select(w => new WorkRunner(w, printer)).ToList();
                    List<WorkPreview> resTask = new List<WorkPreview>();
                    if (Session.MakePreviewInParallel)
                    {
                        var tasks = works.Select(w => w.PreviewAsync(printer));
                        resTask = (await Task.WhenAll(tasks)).ToList();
                    }else
                    {
                        foreach (var work in works)
                            resTask.Add(await work.PreviewAsync(printer));
                    }
                    res.Works = resTask.Select(w =>
                    {
                        w.Session = res;
                        return w;
                    }).ToList();
                    // Run post preview actions
                    foreach (var work in works.Where(w => w.Definition.PostPreviewAction != null))
                        work.Definition.PostPreviewAction(res);
                    // Clean results
                    if (!includeNoneActionItems)
                        res.CleanNoneActions();
                    return res;
                });
        }
        public Task RunAsync(SessionPreview preview, IPrinter printer)
        {
            return printer.IndentAsync("Running session:", async () =>
            {
                // Run order:

                // 1 - Insert 1º level
                // 2 - Update 1º level

                // 3 - Insert 2º level
                // 4 - Update 2º level
                // . - ...............
                // 5 - Insert nº level
                // 6 - Update nº level

                // 7 - Delete nº level
                // . - ...............
                // 8 - Delete 2º level
                // 9 - Delete 1º level

                List<Task> tasks = new List<Task>();
                List<(ICollection<ItemPreview> items, WorkRunner runner)> main = new List<(ICollection<ItemPreview> items, WorkRunner runner)>();
                List<(ICollection<ItemRelationPreview> relations, IItemSideRunner runner)> levels = new List<(ICollection<ItemRelationPreview> relations, IItemSideRunner runner)>();

                foreach (var work in preview.Works)
                {
                    var runWork = works.Single(w => w.Definition.Id == work.Id);
                    main.Add((work.Items, runWork));
                }
                await printer.ForeachAsync("Inserting level 0", main, async m =>
                {
                    levels.AddRange(await ProcessWork(m.items, m.runner, SynchronizationAction.Insert));
                }, false);
                await printer.ForeachAsync("Updating level 0",main, async m =>
                {
                    levels.AddRange(await ProcessWork(m.items, m.runner, SynchronizationAction.Update));
                }, false);
                levels = levels.Distinct().ToList();
                int level = 1;
                while (levels.Any())
                {
                    var aux = levels.ToList();
                    levels.Clear();
                    await printer.ForeachAsync($"Inserting level {level}", aux, async lev =>
                    {
                        levels.AddRange(await ProcessRelations(lev.relations, lev.runner, SynchronizationAction.Insert, level));
                    }, false);
                    await printer.ForeachAsync($"Updating level {level}", aux, async lev =>
                    {
                        levels.AddRange(await ProcessRelations(lev.relations, lev.runner, SynchronizationAction.Update, level));
                    }, false);
                    levels = levels.Distinct().ToList();
                    level++;
                }
                await printer.ForeachAsync("Deleting level 0", main, async m =>
                {
                    levels.AddRange(await ProcessWork(m.items, m.runner, SynchronizationAction.Delete));
                }, false);

                level = 1;
                while (levels.Any())
                {
                    var aux = levels.ToList();
                    levels.Clear();
                    await printer.ForeachAsync($"Deleting level {level}", aux, async lev =>
                    {
                        levels.AddRange(await ProcessRelations(lev.relations, lev.runner, SynchronizationAction.Delete, level));
                    }, false);
                    level++;
                }
                // Run post run actions
                foreach (var work in works.Where(w => w.Definition.PostRunAction != null))
                    work.Definition.PostRunAction(preview);
            });
        }
        private static async Task<ICollection<(ICollection<ItemRelationPreview>, IItemSideRunner)>> ProcessWork(ICollection<ItemPreview> items, WorkRunner runner, SynchronizationAction action)
        {
            ICollection<(ICollection<ItemRelationPreview>, IItemSideRunner)> levels = new List<(ICollection<ItemRelationPreview>, IItemSideRunner)>();
            foreach (var item in items)
            {
                var runItem = runner.Items.Single(i => i.Id == item.Id);

                foreach (var side in item.Sides)
                {
                    var runSide = runner.Sides.Single(s => s.Id == side.Id);
                    var runItemSide = runItem.SideRunners.Single(s => s.Side.Id == side.Id);
                    var map = new Func<IItemRunner, IItemSideRunner, object>((i, s) =>
                    {
                        if (runSide.Comparator.GetItemTypes().typeA == runner.MasterSide.GetItemType())
                        {
                            // Master is A in this comparator
                            if (item.MasterItemExist)
                                return runSide.Comparator.MapAToB(i.MasterItem, s.SideItem);
                        }
                        else
                        {
                            // Master is B in this comparator
                            if (item.MasterItemExist)
                                return runSide.Comparator.MapBToA(i.MasterItem, s.SideItem);
                        }
                        return null;
                    });

                    if (side.IsEnabled && side.Action == action && side.Action == SynchronizationAction.Insert)
                    {
                        var newItem = map(runItem, runItemSide);
                        await runSide.InsertAsync(newItem);
                        foreach (var subItem in runItemSide.SubItems)
                            subItem.SideRunners.First().Side.Source = newItem;
                    }
                    else if (side.IsEnabled && side.Action == action && side.Action == SynchronizationAction.Update)
                    {
                        var newItem = map(runItem, runItemSide);
                        await runSide.UpdateAsync(newItem);
                        foreach (var subItem in runItemSide.SubItems)
                            subItem.SideRunners.First().Side.Source = newItem;
                    }
                    else if (side.IsEnabled && side.Action == action && side.Action == SynchronizationAction.Delete)
                    {
                        await runSide.DeleteAsync(runItemSide.SideItem);
                    }
                    levels.Add((side.Relations, runItemSide));
                }
            }
            return levels;
        }
        private static async Task<ICollection<(ICollection<ItemRelationPreview>, IItemSideRunner)>> ProcessRelations(ICollection<ItemRelationPreview> relations, IItemSideRunner runItemSide, SynchronizationAction action, int level = 1)
        {
            if (!relations.Any()) return Enumerable.Empty<(ICollection<ItemRelationPreview>, IItemSideRunner)>().ToList();
            List<(ICollection<ItemRelationPreview>, IItemSideRunner)> nextLevels = new List<(ICollection<ItemRelationPreview>, IItemSideRunner)>();
            foreach (var rel in relations)
            {
                var runSubItem = runItemSide.SubItems.Single(si => si.Id == rel.Id);
                var runSubItemSide = runSubItem.SideRunners.First();
                var runSubSide = runSubItemSide.Side;
                var subMap = new Func<IItemRunner, IItemSideRunner, object>((i, s) =>
                {
                    if (runSubSide.Comparator.GetItemTypes().typeA.GetTypeInfo().IsAssignableFrom(runSubItem.MasterItem.GetType().GetTypeInfo()))
                    {
                        // Master is A in this comparator
                        if (runSubItem.MasterItem != null)
                            return runSubSide.Comparator.MapAToB(i.MasterItem, s.SideItem);
                    }
                    else
                    {
                        // Master is B in this comparator
                        if (runSubItem.MasterItem != null)
                            return runSubSide.Comparator.MapBToA(i.MasterItem, s.SideItem);
                    }
                    return null;
                });
                if (rel.IsEnabled && rel.Action == action && rel.Action == SynchronizationAction.Insert)
                {
                    var newItem = subMap(runSubItem, runSubItemSide);
                    await runSubSide.InsertAsync(newItem);
                    foreach (var subItem in runSubItemSide.SubItems)
                        subItem.SideRunners.First().Side.Source = newItem;
                }
                else if (rel.IsEnabled && rel.Action == action && rel.Action == SynchronizationAction.Update)
                {
                    var newItem = subMap(runSubItem, runSubItemSide);
                    await runSubSide.UpdateAsync(newItem);
                    foreach (var subItem in runSubItemSide.SubItems)
                        subItem.SideRunners.First().Side.Source = newItem;
                }
                else if (rel.IsEnabled && rel.Action == action && rel.Action == SynchronizationAction.Delete)
                {
                    await runSubSide.DeleteAsync(runSubItemSide.SideItem);
                }
                if (rel.Relations.Any())
                    nextLevels.Add((rel.Relations, runSubItemSide));
            }
            return nextLevels;
        }
    }
}