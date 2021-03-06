﻿using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
using Xunit.Abstractions;
using Xunit;
using Fuxion.Test;
using SimpleInjector;
using Fuxion.Repositories;
using Fuxion.Identity.Test.Repositories;
using Fuxion.Factories;
namespace Fuxion.Identity.Test
{
    public class RolTest : BaseTest
    {
        public RolTest(ITestOutputHelper helper) : base(helper)
        {
            Container c = new Container();

            // TypeDiscriminators
            c.RegisterSingleton(new TypeDiscriminatorFactory().Transform(fac =>
            {
                fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
                return fac;
            }));
            // IdentityManager
            c.Register<IPasswordProvider, PasswordProviderMock>();
            c.RegisterSingleton<ICurrentUserNameProvider>(new GenericCurrentUserNameProvider(() => Context.Rol.Identity.Root.UserName));
            c.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(new IdentityMemoryTestRepository());
            c.Register<IdentityManager>();
            //Factory.AddInjector(new InstanceInjector<IdentityManager>(new IdentityManager()));

            Factory.AddInjector(new SimpleInjectorFactoryInjector(c));
        }
        [Fact(DisplayName = "Rol - Cannot by function")]
        public void CannotByFunction()
        {
            Assert.False(
                new IdentityDao
                {
                    Id = "test",
                    Name = "Test",
                    Groups = new[]
                    {
                        new GroupDao
                        {
                            Id = "admins",
                            Name = "Admins",
                            Permissions = new[]
                            {
                                new PermissionDao {
                                    Value = true,
                                    Function = Edit.Id.ToString(),
                                    Scopes =new[] {
                                        new ScopeDao {
                                            Discriminator = Discriminator.Category.Purchases,
                                            Propagation = ScopePropagation.ToMe | ScopePropagation.ToExclusions }
                                    }
                                }
                            }
                        }
                    }
                }.Can(Delete).Instance(Discriminator.Category.Purchases)
                , "Tengo:\r\n" +
                        $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminator.Category.Purchases)}\r\n" +
                        $"¿Debería poder {nameof(Delete)} en {nameof(Discriminator.Category.Purchases)}?\r\n" +
                        " No");
        }
        [Fact(DisplayName = "pppp")]
        public void Canppppp()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Groups = new[]
                {
                    new GroupDao
                    {
                        Id = "admins",
                        Name = "Admins",
                        Permissions = new[]
                        {
                            new PermissionDao {
                                Value = true,
                                Function = Edit.Id.ToString(),
                                Scopes =new[] {
                                    new ScopeDao {
                                        Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CategoryDao>(),
                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
                                }
                            }
                        }
                    }
                }
            };//.EnsureCan(Read).Instance(Discriminator.Location.City);
            Assert.False(ide.Can(Read).Instance(Discriminator.Location.City.Buffalo),
                 "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Edit)} el tipo {nameof(CategoryDao)} y derivados\r\n" +
                    $"¿Debería poder {nameof(Read)} en {nameof(Discriminator.Location.City.Buffalo)}?\r\n" +
                    " No");
            //Assert.True(ide.EnsureCan(Admin).Something(),
            //     "Tengo:\r\n" +
            //        $" - Concedido el permiso para {nameof(Admin)} la categoria {Discriminator.Category.Purchases} del tipo {nameof(BaseDao)} y derivados\r\n" +
            //        $"¿Debería poder {nameof(Admin)} alguna cosa?\r\n" +
            //        " Si");
        }
        [Fact(DisplayName = "Rol - Can by instance multiple properties")]
        public void CanByInstance2()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Groups = new[]
                {
                    new GroupDao
                    {
                        Id = "admins",
                        Name = "Admins",
                        Permissions = new[]
                        {
                            new PermissionDao {
                                Value = true,
                                Function = Edit.Id.ToString(),
                                Scopes =new[] {
                                    new ScopeDao {
                                        Discriminator = Discriminator.Category.Purchases,
                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToExclusions }
                                }
                            }
                        }
                    }
                }
            }.EnsureCan(Edit).Instance(Discriminator.Category.Purchases);
        }
        [Fact(DisplayName = "Rol - Can by instance")]
        public void CanByInstance()
        {
            new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Groups = new[]
                {
                    new GroupDao
                    {
                        Id = "admins",
                        Name = "Admins",
                        Permissions = new[]
                        {
                            new PermissionDao {
                                Value = true,
                                Function =Edit.Id.ToString(),
                                Scopes =new[] {
                                    new ScopeDao {
                                        Discriminator = Discriminator.Location.State.California,
                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToExclusions }
                                }
                            }
                        }
                    }
                }
            }.EnsureCan(Edit).AllLocations(Discriminator.Location.Country.Usa);
            //}.EnsureCan(Edit).Instance(Discriminator.Location.Country.Usa);
        }
        [Fact(DisplayName = "Rol - Cannot by instance")]
        public void CannotByInstance()
        {
            Assert.False(
                new IdentityDao
                {
                    Id = "test",
                    Name = "Test",
                    Groups = new[]
                    {
                        new GroupDao
                        {
                            Id = "admins",
                            Name = "Admins",
                            Permissions = new[]
                            {
                                new PermissionDao {
                                    Value = true,
                                    Function =Edit.Id.ToString(),
                                    Scopes =new[] {
                                        new ScopeDao {
                                            Discriminator = Discriminator.Category.Purchases,
                                            Propagation = ScopePropagation.ToMe }
                                    }
                                }
                            }
                        }
                    }
                }.Can(Edit).Instance(File.Document.Word.Word1));
        }
        [Fact(DisplayName = "Rol - Can by type, same discriminator type")]
        public void CanByType()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value =true,
                        Function = Admin.Id.ToString(),
                        Scopes =new[]{
                            new ScopeDao {
                                Discriminator = Discriminator.Category.Purchases,
                                Propagation = ScopePropagation.ToMe },
                            new ScopeDao {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<BaseDao>(),
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions },
                        }
                    },
                }.ToList()
            };
            ide.EnsureCan(Read).Type<WordDocumentDao>();
            Assert.True(ide.EnsureCan(Read).Type<WordDocumentDao>(),
                 "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Admin)} la categoria {Discriminator.Category.Purchases} del tipo {nameof(BaseDao)} y derivados\r\n" +
                    $"¿Debería poder {nameof(Read)} en {nameof(WordDocumentDao)}?\r\n" +
                    " Si");
            Assert.True(ide.EnsureCan(Admin).Something(),
                 "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Admin)} la categoria {Discriminator.Category.Purchases} del tipo {nameof(BaseDao)} y derivados\r\n" +
                    $"¿Debería poder {nameof(Admin)} alguna cosa?\r\n" +
                    " Si");
        }
        [Fact(DisplayName = "Rol - Can by type, different discriminator type")]
        public void CanByType2()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value =true,
                        Function = Admin.Id.ToString(),
                        Scopes =new[]{
                            new ScopeDao {
                                Discriminator = Discriminator.Category.Purchases,
                                Propagation = ScopePropagation.ToMe },
                        }
                    },
                }.ToList()
            };
            Assert.True(ide.Can(Read).Type<WordDocumentDao>(), "\r\n" +
                $" - Granted permission for '{nameof(Admin)}' anything of category '{nameof(Discriminator.Category.Purchases)}'\r\n" +
                $"Should i be able to '{nameof(Read)}' objects of type '{nameof(WordDocumentDao)}'?\r\n" +
                $"YES");
        }
        [Fact(DisplayName = "Rol - Can by instance, mix root permission with others")]
        public void CanByType3()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Admin.Id.ToString(),
                    },
                    new PermissionDao {
                        Value =true,
                        Function = Admin.Id.ToString(),
                        Scopes =new[]{
                            new ScopeDao {
                                Discriminator = Discriminator.Category.Purchases,
                                Propagation = ScopePropagation.ToMe },
                        }
                    },
                }.ToList()
            };
            Assert.True(ide.EnsureCan(Read).Instance(File.Document.Word.Word1),
                 "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Admin)} en {nameof(Discriminator.Category.Purchases)}\r\n" +
                    $"¿Debería poder {nameof(Read)} en {nameof(File.Document.Word.Word1)}?\r\n" +
                    " Si");
            Assert.True(ide.EnsureCan(Read).Something(),
                 "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Admin)} en {nameof(Discriminator.Category.Purchases)}\r\n" +
                    $"¿Debería poder {nameof(Read)} alguna cosa?\r\n" +
                    " Si");
        }
        [Fact(DisplayName = "Rol - Cannot by type")]
        public void CannotByType()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Edit.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao {
                                Discriminator = Discriminator.Location.City.SanFrancisco,
                                Propagation = ScopePropagation.ToMe } } },
                    new PermissionDao {
                        Value = false,
                        Function = Edit.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao {
                                Discriminator = Discriminator.Location.State.California,
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions } } }
                }.ToList()
            };
            Assert.False(ide.IsRoot(),
                "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminator.Location.City.SanFrancisco)}\r\n" +
                    $" - Denegado el permiso para {nameof(Edit)} en {nameof(Discriminator.Location.State.California)} y sus hijos\r\n" +
                    $"¿Debería ser root?\r\n" +
                    " No");
            Assert.False(ide.Can(Edit).Instance(Discriminator.Location.City.SanFrancisco)
                , "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminator.Location.City.SanFrancisco)}\r\n" +
                    $" - Denegado el permiso para {nameof(Edit)} en {nameof(Discriminator.Location.State.California)} y sus hijos\r\n" +
                    $"¿Debería poder {nameof(Edit)} en {nameof(Discriminator.Location.City.SanFrancisco)}?\r\n" +
                    " No");
        }
        [Fact(DisplayName = "Rol - No permissions")]
        public void NoPermissions()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
            };
            Assert.False(ide.IsRoot(),
                "Tengo:\r\n" +
                    $" - Ningún permiso\r\n" +
                    $"¿Debería ser root?\r\n" +
                    " No");
            Assert.False(ide.Can(Read).Something(),
                "Tengo:\r\n" +
                    $" - Ningún permiso\r\n" +
                    $"¿Debería poder '{nameof(Read)}' alguna cosa?\r\n" +
                    " No");
            Assert.False(ide.Can(Edit).Instance(Discriminator.Location.City.SanFrancisco)
                , "Tengo:\r\n" +
                    $" - Ningun permiso\r\n" +
                    $"¿Debería poder {nameof(Edit)} en {nameof(Discriminator.Location.City.SanFrancisco)}?\r\n" +
                    " No");
        }
        [Fact(DisplayName = "Rol - Root permission")]
        public void RootPermission()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Admin.Id.ToString(),
                    }
                }.ToList()
            };
            Assert.True(ide.IsRoot(),
                "Tengo:\r\n" +
                    $" - Permiso ROOT\r\n" +
                    $"¿Debería ser root?\r\n" +
                    " Si");
            Assert.True(ide.Can(Admin).Anything(),
                "Tengo:\r\n" +
                    $" - Permiso ROOT\r\n" +
                    $"¿Debería poder '{nameof(Admin)}' cualquier cosa?\r\n" +
                    " Si");
            Assert.True(ide.Can(Admin).Something(),
                "Tengo:\r\n" +
                    $" - Permiso ROOT\r\n" +
                    $"¿Debería poder '{nameof(Admin)}' alguna cosa?\r\n" +
                    " Si");
            Assert.True(ide.Can(Edit).Instance(File.Document.Word.Word1),
                "Tengo:\r\n" +
                    $" - Permiso ROOT\r\n" +
                    $"¿Debería poder '{nameof(Edit)}' en '{nameof(File.Document.Word.Word1)}'?\r\n" +
                    " Si");
            Assert.True(ide.Can(Edit).Type<CityDao>(),
                "Tengo:\r\n" +
                    $" - Permiso ROOT\r\n" +
                    $"¿Debería poder '{nameof(Edit)}' las '{nameof(CityDao)}'?\r\n" +
                    " Si");
        }
    }
}
