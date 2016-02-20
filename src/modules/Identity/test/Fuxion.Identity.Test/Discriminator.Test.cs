﻿using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Identity.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fuxion.Identity.Test.Entity;
using static Fuxion.Identity.Functions;

namespace Fuxion.Identity.Test
{
    [TestClass]
    public class DiscriminatorTest : BaseTestClass
    {
        [TestMethod]
        public void Validate_GuidDiscriminator()
        {
            var ttt = new GuidDiscriminator(default(Guid),       "valid", Guid.NewGuid(), "valid");

            // Invalid by Id
            Assert.IsFalse(new GuidDiscriminator(default(Guid) , "valid", Guid.NewGuid(), "valid").IsValid());

            // Invalid by Name
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), null   , Guid.NewGuid(), "valid").IsValid());
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), ""     , Guid.NewGuid(), "valid").IsValid());
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), " "    , Guid.NewGuid(), "valid").IsValid());

            // Invalid by TypeId
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), "valid", default(Guid) , "valid").IsValid());

            // Invalid by TypeName
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), "valid", Guid.NewGuid(), null).IsValid());
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), "valid", Guid.NewGuid(), "").IsValid());
            Assert.IsFalse(new GuidDiscriminator(Guid.NewGuid(), "valid", Guid.NewGuid(), " ").IsValid());

            // Valid
            Assert.IsTrue(new GuidDiscriminator(Guid.NewGuid() , "valid", Guid.NewGuid(), "valid").IsValid());
        }
        [TestMethod]
        public void Validate_StringDiscriminator()
        {
            // Name and Id are same value
            // Invalid by Name
            Assert.IsFalse(new StringDiscriminator(null, null, null, "valid").IsValid());
            Assert.IsFalse(new StringDiscriminator(""  , null, null, "valid").IsValid());
            Assert.IsFalse(new StringDiscriminator(" " , null, null, "valid").IsValid());

            // TypeName and TypeId are same value
            // Invalid by TypeName
            Assert.IsFalse(new StringDiscriminator("valid", null, null, null).IsValid());
            Assert.IsFalse(new StringDiscriminator("valid", null, null, ""  ).IsValid());
            Assert.IsFalse(new StringDiscriminator("valid", null, null, " ").IsValid());

            // Valid
            Assert.IsTrue(new StringDiscriminator("valid", null, null, "valid").IsValid());
        }
        [TestMethod]
        public void Validate_GuidStringDiscriminator()
        {
            // Invalid by Id
            Assert.IsFalse(new GuidStringDiscriminator(default(Guid), "valid", null, null, "valid").IsValid());

            // Invalid by Name
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), null, null, null, "valid").IsValid());
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), "", null, null, "valid").IsValid());
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), " ", null, null, "valid").IsValid());

            // TypeName and TypeId are same value
            // Invalid by Typename
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, null).IsValid());
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, "").IsValid());
            Assert.IsFalse(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, " ").IsValid());

            // Valid
            Assert.IsTrue(new GuidStringDiscriminator(Guid.NewGuid(), "valid", null, null, "valid").IsValid());
        }

        [TestMethod]
        public void Compare_GuidDiscriminator()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var comparer = new DiscriminatorEqualityComparer();
            Assert.IsFalse(comparer.Equals(
                new GuidDiscriminator(id1, null, id3, null),
                new GuidDiscriminator(id2, null, id3, null)),
                "The comparison should fail if identifications(Id) differ");
            Assert.IsFalse(comparer.Equals(
                new GuidDiscriminator(id1, null, id2, null),
                new GuidDiscriminator(id1, null, id3, null)),
                "The comparison should fail if type identifications(TypeId) differ");
            Assert.IsTrue(comparer.Equals(
                new GuidDiscriminator(id1, null, id3, null),
                new GuidDiscriminator(id1, null, id3, null)),
                "The comparison should pass if identifications(Id) and type identifications(TypeId) are same");
        }
        [TestMethod]
        public void Compare_StringDiscriminator()
        {
            var id1 = "id1";
            var id2 = "id2";
            var id3 = "id3";
            var comparer = new DiscriminatorEqualityComparer();
            Assert.IsFalse(comparer.Equals(
                new StringDiscriminator(id1, null, null, id3),
                new StringDiscriminator(id2, null, null, id3)),
                "The comparison should fail if identifications(Id) differ");
            Assert.IsFalse(comparer.Equals(
                new StringDiscriminator(id1, null, null, id2),
                new StringDiscriminator(id1, null, null, id3)),
                "The comparison should fail if type identifications(TypeId) differ");
            Assert.IsTrue(comparer.Equals(
                new StringDiscriminator(id1, null, null, id3),
                new StringDiscriminator(id1, null, null, id3)),
                "The comparison should pass if identifications(Id) and type identifications(TypeId) are same");
        }
        [TestMethod]
        public void GetScopes_RolFiltered()
        {
            var rol = new Rol
            {
                Id = "Tom",
                Permissions = new[] {
                    new Permission {
                        Value =true,
                        Function = Read.Id.ToString(),
                        Scopes =new[] {
                            new Scope {
                                Discriminator = new Country {
                                    Id = "USA",
                                    Name = "USA",
                                },
                                Propagation = ScopePropagation.ToMe
                            }
                        }
                    }
                }.ToList()
            };
            
            var res = rol.GetDiscriminators(Read, new[] { TypeDiscriminator.Create<Location>() }, Printer.Print);
            Printer.Print(0, $"Se han seleccionado '{res.Count()}' scopes:");
            foreach (var sco in res)
                Printer.Print(3, sco.ToString());
            Assert.IsTrue(res.Any());
        }
    }
}
