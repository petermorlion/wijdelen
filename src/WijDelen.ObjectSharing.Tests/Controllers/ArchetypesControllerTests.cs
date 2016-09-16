using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Localization;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.Fakes;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ArchetypesControllerTests {
        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT()
        {
            var controller = new ArchetypesController(null, null);
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void IndexShouldShowArchetypes() {
            var records = new[] {
                new ItemArchetypeRecord(),
                new ItemArchetypeRecord()
            };

            var repositoryMock = new Mock<IRepository<ItemArchetypeRecord>>();
            repositoryMock.SetRecords(records);

            var controller = new ArchetypesController(repositoryMock.Object, null);

            var result = controller.Index();

            ((ViewResult)result).Model.ShouldBeEquivalentTo(records);
        }

        [Test]
        public void UnarchetypedShouldShowUnarchetypedSynonyms() {
            var synonyms = new[] {
                new ArchetypedSynonymRecord {
                    Archetype = "Sneakers"
                }
            };

            var synonymsRepositoryMock = new Mock<IRepository<ArchetypedSynonymRecord>>();
            synonymsRepositoryMock.SetRecords(synonyms);

            var archetypes = new[] {
                new ItemArchetypeRecord { Name = "Ladder" },
                new ItemArchetypeRecord { Name= "Sneakers" }
            };

            var archetypesRepositoryMock = new Mock<IRepository<ItemArchetypeRecord>>();
            archetypesRepositoryMock.SetRecords(archetypes);

            var controller = new ArchetypesController(archetypesRepositoryMock.Object, synonymsRepositoryMock.Object);

            var result = controller.Synonyms();

            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Record.Should().Be(synonyms[0]);
            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].SelectedArchetype.Should().Be(archetypes[1]);
            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Archetypes.Should().BeEquivalentTo(archetypes);
        }
    }
}