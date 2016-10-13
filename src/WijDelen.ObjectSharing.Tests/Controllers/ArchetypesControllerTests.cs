//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web.Mvc;
//using FluentAssertions;
//using Moq;
//using NUnit.Framework;
//using Orchard.Data;
//using Orchard.Localization;
//using WijDelen.ObjectSharing.Controllers;
//using WijDelen.ObjectSharing.Domain.Commands;
//using WijDelen.ObjectSharing.Domain.Messaging;
//using WijDelen.ObjectSharing.Models;
//using WijDelen.ObjectSharing.Tests.Fakes;
//using WijDelen.ObjectSharing.ViewModels;

//namespace WijDelen.ObjectSharing.Tests.Controllers {
//    [TestFixture]
//    public class ArchetypesControllerTests {
//        /// <summary>
//        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
//        /// </summary>
//        [Test]
//        public void TestT()
//        {
//            var controller = new ArchetypesController(null, null, null, null);
//            var localizer = NullLocalizer.Instance;

//            controller.T = localizer;

//            Assert.AreEqual(localizer, controller.T);
//        }

//        [Test]
//        public void IndexShouldShowArchetypes() {
//            var records = new[] {
//                new ArchetypePartRecord(),
//                new ArchetypePartRecord()
//            };

//            var repositoryMock = new Mock<IRepository<ArchetypePartRecord>>();
//            repositoryMock.SetRecords(records);

//            var controller = new ArchetypesController(repositoryMock.Object, null, null, null);

//            var result = controller.Index();

//            ((ViewResult)result).Model.ShouldBeEquivalentTo(records);
//        }

//        [Test]
//        public void SynonmysShouldShowSynonyms() {
//            var synonyms = new[] {
//                new ArchetypedSynonymRecord {
//                    Archetype = "Sneakers",
//                    Synonym = "Sporting shoes"
//                }
//            };

//            var synonymsRepositoryMock = new Mock<IRepository<ArchetypedSynonymRecord>>();
//            synonymsRepositoryMock.SetRecords(synonyms);

//            var archetypes = new[] {
//                new ArchetypePartRecord { AggregateId = Guid.NewGuid(), Name = "Ladder" },
//                new ArchetypePartRecord { AggregateId = Guid.NewGuid(), Name = "Sneakers" }
//            };

//            var archetypesRepositoryMock = new Mock<IRepository<ArchetypePartRecord>>();
//            archetypesRepositoryMock.SetRecords(archetypes);

//            var controller = new ArchetypesController(archetypesRepositoryMock.Object, synonymsRepositoryMock.Object, null, null);

//            var result = controller.Synonyms();

//            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Synonym.Should().Be("Sporting shoes");
//            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].SelectedArchetypeId.Should().Be(archetypes[1].AggregateId.ToString());
//            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Archetypes.Count.Should().Be(2);
//            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Archetypes[0].Id.Should().Be(archetypes[0].AggregateId);
//            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Archetypes[0].Name.Should().Be(archetypes[0].Name);
//            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Archetypes[1].Id.Should().Be(archetypes[1].AggregateId);
//            ((ViewResult)result).Model.As<SynonymsViewModel>().Synonyms[0].Archetypes[1].Name.Should().Be(archetypes[1].Name);
//        }

//        [Test]
//        public void WhenPostingNewArchetype() {
//            var viewModel = new CreateArchetypeViewModel { Name = "Sneakers" };
            
//            var commandHandlerMock = new Mock<ICommandHandler<CreateArchetype>>();
//            CreateArchetype command = null;
//            commandHandlerMock.Setup(x => x.Handle(It.IsAny<CreateArchetype>())).Callback((CreateArchetype c) => command = c);

//            var controller = new ArchetypesController(null, null, commandHandlerMock.Object, null);

//            var result = controller.Create(viewModel);

//            ((RedirectToRouteResult)result).RouteValues["action"].Should().Be("Index");

//            command.Name.Should().Be("Sneakers");
//        }

//        [Test]
//        public void WhenPostingNewArchetypeWithoutName_ShouldReturnError() {
//            var viewModel = new CreateArchetypeViewModel();
            
//            var controller = new ArchetypesController(null, null, null, null);

//            var result = controller.Create(viewModel);

//            ((ViewResult)result).ViewData.ModelState["Name"].Errors.Single().ErrorMessage.Should().Be("Please provide a name for the archetype.");
//        }

//        [Test]
//        public void WhenPostingArchetypesAndSynonyms() {
//            SetSynonymArchetypes command = null;
//            var commandHandlerMock = new Mock<ICommandHandler<SetSynonymArchetypes>>();
//            commandHandlerMock
//                .Setup(x => x.Handle(It.IsAny<SetSynonymArchetypes>()))
//                .Callback((SetSynonymArchetypes c) => command = c);

//            var controller = new ArchetypesController(null, null, null, commandHandlerMock.Object);
//            var selectedArchetypeId = Guid.NewGuid();
//            var viewModel = new SynonymsViewModel {
//                Synonyms = new List<EditArchetypedSynonymViewModel> {
//                    new EditArchetypedSynonymViewModel {
//                        Synonym = "Sporting shoes",
//                        SelectedArchetypeId = selectedArchetypeId.ToString()
//                    }
//                }
//            }; 

//            var result = controller.Synonyms(viewModel);

//            ((RedirectToRouteResult)result).RouteValues["action"].Should().Be("Synonyms");

//            command.ArchetypeSynonyms[selectedArchetypeId].Single().Should().Be("Sporting shoes");
//        }
//    }
//}