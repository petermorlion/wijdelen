using System;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Security;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ObjectRequestResponseControllerTests {
        [Test]
        public void WhenGettingNoForForUnknownId_ShouldReturnNotFound() {
            var id = Guid.NewGuid();

            var persistentRecords = new ObjectRequestRecord[0];

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, null, null);

            var actionResult = controller.NoFor(id);

            actionResult.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void WhenGettingNoFor_ShouldPersistNoAndReturnView() {
            var id = Guid.NewGuid();

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(22);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;

            var persistentRecords = new[] {
                new ObjectRequestRecord {
                    AggregateId = id,
                    UserId = 22
                }
            };

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, services, null, null);

            var actionResult = controller.NoFor(id);

            actionResult.Should().BeOfType<ViewResult>();
        }

        [Test]
        public void WhenGettingYesForForUnknownId_ShouldReturnNotFound() {
            var id = Guid.NewGuid();

            var persistentRecords = new ObjectRequestRecord[0];

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, null, null);

            var actionResult = controller.NoFor(id);

            actionResult.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void WhenGettingYesFor_ShouldPersistNoAndReturnView()
        {
            var id = Guid.NewGuid();

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(22);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;

            var persistentRecords = new[] {
                new ObjectRequestRecord {
                    AggregateId = id,
                    UserId = 666,
                    Description = "Sneakers"
                }
            };

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var sneakers = new SynonymFactory().Create("Sneakers");
            var queryMock = new Mock<IFindSynonymsByExactMatchQuery>();
            queryMock.Setup(x => x.GetResults("Sneakers")).Returns(new[] { sneakers });

            MarkSynonymAsOwned markSynonymAsOwned = null;
            var commandHandlerMock = new Mock<ICommandHandler<MarkSynonymAsOwned>>();
            commandHandlerMock.Setup(x => x.Handle(It.IsAny<MarkSynonymAsOwned>())).Callback((MarkSynonymAsOwned e) => markSynonymAsOwned = e);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, services, queryMock.Object, commandHandlerMock.Object);

            var actionResult = controller.YesFor(id);

            actionResult.Should().BeOfType<ViewResult>();
            markSynonymAsOwned.UserId.Should().Be(22);
            markSynonymAsOwned.SynonymId.Should().Be(sneakers.Id);
            markSynonymAsOwned.SynonymTitle.Should().Be("Sneakers");
        }

        [Test]
        public void WhenGettingNotNowForForUnknownId_ShouldReturnNotFound() {
            var id = Guid.NewGuid();

            var persistentRecords = new ObjectRequestRecord[0];

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, null, null);

            var actionResult = controller.NoFor(id);

            actionResult.Should().BeOfType<HttpNotFoundResult>();
        }
    }
}