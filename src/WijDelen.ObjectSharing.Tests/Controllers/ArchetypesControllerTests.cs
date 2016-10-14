using System.Collections.Generic;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;
using Orchard.Localization;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ArchetypesControllerTests {
        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var controller = new ArchetypesController(Mock.Of<IFindAllArchetypesQuery>(), Mock.Of<IFindAllSynonymsQuery>());
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void IndexShouldShowArchetypes()
        {
            var archetypeFactory = new ArchetypeFactory();

            var archetypes = new[] {
                archetypeFactory.Create("Sneakers"),
                archetypeFactory.Create("Flaming Moe")
            };

            var archetypesQueryMock = new Mock<IFindAllArchetypesQuery>();
            archetypesQueryMock.Setup(x => x.GetResult()).Returns(archetypes);

            var controller = new ArchetypesController(archetypesQueryMock.Object, Mock.Of<IFindAllSynonymsQuery>());
            var result = controller.Index();

            ((ViewResult)result).Model.ShouldBeEquivalentTo(archetypes);
        }

        [Test]
        public void SynonmysShouldShowSynonyms()
        {
            var archetypeFactory = new ArchetypeFactory();

            var archetypes = new[] {
                archetypeFactory.Create("Sneakers"),
                archetypeFactory.Create("Ladder")
            };

            var archetypesQueryMock = new Mock<IFindAllArchetypesQuery>();
            archetypesQueryMock.Setup(x => x.GetResult()).Returns(archetypes);

            var synonymFactory = new SynonymFactory();
            var synonyms = new[] {
                synonymFactory.Create("Sporting shoes", archetypes[0].Id)
            };

            var synonymsQueryMock = new Mock<IFindAllSynonymsQuery>();
            synonymsQueryMock.Setup(x => x.GetResult()).Returns(synonyms);

            var controller = new ArchetypesController(archetypesQueryMock.Object, synonymsQueryMock.Object);

            var result = controller.Synonyms();

            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Synonym.Should().Be("Sporting shoes");
            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].SelectedArchetypeId.Should().Be(archetypes[0].Id);
            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Archetypes.Count.Should().Be(2);
            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Archetypes[0].Should().Be(archetypes[0]);
            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Archetypes[1].Should().Be(archetypes[1]);
        }

        [Test]
        public void WhenChangingArchetypeForSynonym()
        {
            var archetypeFactory = new ArchetypeFactory();

            var archetypes = new[] {
                archetypeFactory.Create("Sneakers"),
                archetypeFactory.Create("Ladder")
            };

            var archetypesQueryMock = new Mock<IFindAllArchetypesQuery>();
            archetypesQueryMock.Setup(x => x.GetResult()).Returns(archetypes);

            var synonymFactory = new SynonymFactory();
            var synonyms = new[] {
                synonymFactory.Create("Sporting shoes", archetypes[0].Id)
            };

            var synonymsQueryMock = new Mock<IFindAllSynonymsQuery>();
            synonymsQueryMock.Setup(x => x.GetResult()).Returns(synonyms);

            var controller = new ArchetypesController(archetypesQueryMock.Object, synonymsQueryMock.Object);

            var viewModel = new SynonymsViewModel
            {
                Synonyms = new List<EditArchetypedSynonymViewModel> {
                    new EditArchetypedSynonymViewModel {
                        Synonym = "Sporting shoes",
                        SelectedArchetypeId = archetypes[1].Id
                    }
                }
            };

            var result = controller.Synonyms(viewModel);

            ((RedirectToRouteResult)result).RouteValues["action"].Should().Be("Synonyms");

            ((ContentPickerField)((ContentPart)synonyms[0].Content.Synonym).Get(typeof(ContentPickerField), "Archetype")).Ids.ShouldBeEquivalentTo(new[] { archetypes[1].Id });
        }

        [Test]
        public void WhenRemovingArchetypeForSynonym()
        {
            var archetypeFactory = new ArchetypeFactory();

            var archetypes = new[] {
                archetypeFactory.Create("Sneakers"),
                archetypeFactory.Create("Ladder")
            };

            var archetypesQueryMock = new Mock<IFindAllArchetypesQuery>();
            archetypesQueryMock.Setup(x => x.GetResult()).Returns(archetypes);

            var synonymFactory = new SynonymFactory();
            var synonyms = new[] {
                synonymFactory.Create("Sporting shoes", archetypes[0].Id)
            };

            var synonymsQueryMock = new Mock<IFindAllSynonymsQuery>();
            synonymsQueryMock.Setup(x => x.GetResult()).Returns(synonyms);

            var controller = new ArchetypesController(archetypesQueryMock.Object, synonymsQueryMock.Object);

            var viewModel = new SynonymsViewModel
            {
                Synonyms = new List<EditArchetypedSynonymViewModel> {
                    new EditArchetypedSynonymViewModel {
                        Synonym = "Sporting shoes",
                        SelectedArchetypeId = null
                    }
                }
            };

            var result = controller.Synonyms(viewModel);

            ((RedirectToRouteResult)result).RouteValues["action"].Should().Be("Synonyms");

            ((ContentPickerField)((ContentPart)synonyms[0].Content.Synonym).Get(typeof(ContentPickerField), "Archetype")).Ids.ShouldBeEquivalentTo(new int[0]);
        }
    }
}