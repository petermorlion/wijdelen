using System.Collections.Generic;
using System.Web.Http.Results;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Controllers.Api;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.Fakes;

namespace WijDelen.ObjectSharing.Tests.Controllers.Api {
    [TestFixture]
    public class ArchetypesControllerTests {
        [Test]
        public void GetShouldReturnExactMatch()
        {
            var records = new[] {
                new ArchetypeRecord { Name = "Sneakers" },
                new ArchetypeRecord { Name = "Flaming Moe" }
            };

            var repositoryMock = new Mock<IRepository<ArchetypeRecord>>();
            repositoryMock.SetRecords(records);

            var controller = new ArchetypesController(repositoryMock.Object, Mock.Of<IRepository<ArchetypedSynonymRecord>>());

            var result = (OkNegotiatedContentResult<List<string>>)controller.Get("sneakers");

            result.Content.ShouldBeEquivalentTo(new List<string> { "Sneakers" });
        }

        [Test]
        public void GetShouldReturnPartialMatches()
        {
            var records = new[] {
                new ArchetypeRecord { Name = "Sneakers" },
                new ArchetypeRecord { Name = "Flaming Moe" },
                new ArchetypeRecord { Name = "Sneaky snake" }
            };

            var repositoryMock = new Mock<IRepository<ArchetypeRecord>>();
            repositoryMock.SetRecords(records);

            var controller = new ArchetypesController(repositoryMock.Object, Mock.Of<IRepository<ArchetypedSynonymRecord>>());

            var result = (OkNegotiatedContentResult<List<string>>)controller.Get("snea");

            result.Content.ShouldBeEquivalentTo(new List<string> { "Sneakers", "Sneaky snake" });
        }

        [Test]
        public void GetShouldReturnNothingIfNoMatches()
        {
            var records = new[] {
                new ArchetypeRecord { Name = "Sneakers" },
                new ArchetypeRecord { Name = "Flaming Moe" },
                new ArchetypeRecord { Name = "Sneaky snake" }
            };

            var repositoryMock = new Mock<IRepository<ArchetypeRecord>>();
            repositoryMock.SetRecords(records);

            var controller = new ArchetypesController(repositoryMock.Object, Mock.Of<IRepository<ArchetypedSynonymRecord>>());

            var result = (OkNegotiatedContentResult<List<string>>)controller.Get("ladder");

            result.Content.ShouldBeEquivalentTo(new List<string>());
        }
        
        [Test]
        public void GetShouldReturnSynonymMatches()
        {
            var archetypes = new[] {
                new ArchetypeRecord { Name = "Sneakers" },
                new ArchetypeRecord { Name = "Flaming Moe" },
                new ArchetypeRecord { Name = "Sneaky snake" }
            };

            var archetypeRepositoryMock = new Mock<IRepository<ArchetypeRecord>>();
            archetypeRepositoryMock.SetRecords(archetypes);

            var synonyms = new[] {
                new ArchetypedSynonymRecord {Archetype = "Sneakers", Synonym = "Sporting shoes"}
            };

            var synonymRepositoryMock = new Mock<IRepository<ArchetypedSynonymRecord>>();
            synonymRepositoryMock.SetRecords(synonyms);

            var controller = new ArchetypesController(archetypeRepositoryMock.Object, synonymRepositoryMock.Object);

            var result = (OkNegotiatedContentResult<List<string>>)controller.Get("spor");

            result.Content.ShouldBeEquivalentTo(new List<string> { "Sneakers" });
        }
        
        [Test]
        public void GetShouldNotReturnDuplicates()
        {
            var archetypes = new[] {
                new ArchetypeRecord { Name = "Sneakers" },
                new ArchetypeRecord { Name = "Flaming Moe" },
                new ArchetypeRecord { Name = "Sneaky snake" }
            };

            var archetypeRepositoryMock = new Mock<IRepository<ArchetypeRecord>>();
            archetypeRepositoryMock.SetRecords(archetypes);

            var synonyms = new[] {
                new ArchetypedSynonymRecord {Archetype = "Sneakers", Synonym = "Sneaky sneakers"}
            };

            var synonymRepositoryMock = new Mock<IRepository<ArchetypedSynonymRecord>>();
            synonymRepositoryMock.SetRecords(synonyms);

            var controller = new ArchetypesController(archetypeRepositoryMock.Object, synonymRepositoryMock.Object);

            var result = (OkNegotiatedContentResult<List<string>>)controller.Get("sneake");

            result.Content.ShouldBeEquivalentTo(new List<string> { "Sneakers" });
        }
    }
}