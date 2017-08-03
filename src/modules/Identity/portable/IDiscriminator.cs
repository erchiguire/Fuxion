﻿using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
using System.Reflection;
using Fuxion.Reflection;
namespace Fuxion.Identity
{
    public interface IDiscriminator : IInclusive<IDiscriminator>, IExclusive<IDiscriminator>
    {
        //IEnumerable<object> Inclusions { get; }
        //IEnumerable<object> Exclusions { get; }
        object TypeId { get; }
        string TypeName { get; }
        object Id { get; }
        string Name { get; }
    }
    public interface IDiscriminator<TId, TTypeId> : IDiscriminator, IInclusive<IDiscriminator<TId, TTypeId>>, IExclusive<IDiscriminator<TId, TTypeId>>
    {
        //new IEnumerable<TId> Inclusions { get; }
        //new IEnumerable<TId> Exclusions { get; }
        new TTypeId TypeId { get; }
        new TId Id { get; }
    }
    public class Discriminator : IDiscriminator
    {
        private Discriminator() { }
        public object TypeId { get; private set; }

        public string TypeName { get; private set; }

        public object Id => "<--- EMPTY --->";

        public string Name => "EMPTY";

        public IEnumerable<IDiscriminator> Inclusions => throw new NotImplementedException();

        public IEnumerable<IDiscriminator> Exclusions => throw new NotImplementedException();

        public static IDiscriminator Empty<TDiscriminator>()
        {
            var att = typeof(TDiscriminator).GetTypeInfo().GetCustomAttribute<DiscriminatorAttribute>();
            if (att != null)
                return new Discriminator
                {
                    TypeId = att.TypeId,
                    TypeName = typeof(TDiscriminator).Name,
                };
            throw new ArgumentException($"The type '{typeof(TDiscriminator).Name}' isn't adorned with Discriminator attribute");
        }
    }
    public static class DiscriminatorExtensions
    {
        public static string ToOneLineString(this IDiscriminator me)
        {
            return $"{me.TypeId} - {me.Id}";
        }
        public static bool IsValid(this IDiscriminator me)
        {
            return
                !Comparer.AreEquals(me.Id, me.Id?.GetType().GetDefaultValue())
                && !string.IsNullOrWhiteSpace(me.Name)
                && !Comparer.AreEquals(me.TypeId, me.TypeId?.GetType().GetDefaultValue())
                && !string.IsNullOrWhiteSpace(me.TypeName);
        }
        public static void Print(this IEnumerable<IDiscriminator> me, PrintMode mode)
        {
            switch (mode)
            {
                case PrintMode.OneLine:
                    break;
                case PrintMode.PropertyList:
                    break;
                case PrintMode.Table:
                    var typeId = me.Select(s => s.TypeId.ToString().Length).Union(new[] { "TYPE_ID".Length }).Max();
                    var typeName = me.Select(s => s.TypeName.Length).Union(new[] { "TYPE_NAME".Length }).Max();
                    var id = me.Select(s => s.Id.ToString().Length).Union(new[] { "ID".Length }).Max();
                    var name = me.Select(s => s.Name.Length).Union(new[] { "ID".Length }).Max();
                    Printer.WriteLine("┌" + ("".PadRight(typeId, '─')) + "┬" + ("".PadRight(typeName, '─')) + "┬" + ("".PadRight(id, '─')) + "┬" + ("".PadRight(name, '─')) + "┐");
                    Printer.WriteLine("│" + "TYPE_ID".PadRight(typeId, ' ') + "│" + "TYPE_NAME".PadRight(typeName, ' ') + "│" + "ID".PadRight(id, ' ') + "│" + "NAME".PadRight(name, ' ') + "│");
                    Printer.WriteLine("├" + ("".PadRight(typeId, '─')) + "┼" + ("".PadRight(typeName, '─')) + "┼" + ("".PadRight(id, '─')) + "┼" + ("".PadRight(name, '─')) + "┤");
                    foreach (var sco in me) Printer.WriteLine("│" + sco.TypeId.ToString().PadRight(typeId, ' ') + "│" + sco.TypeName.PadRight(typeName, ' ') + "│" + sco.Id.ToString().PadRight(id, ' ') + "│" + sco.Name.PadRight(name, ' ') + "│");
                    Printer.WriteLine("└" + ("".PadRight(typeId, '─')) + "┴" + ("".PadRight(typeName, '─')) + "┴" + ("".PadRight(id, '─')) + "┴" + ("".PadRight(name, '─')) + "┘");
                    break;
            }
        }
    }
}
