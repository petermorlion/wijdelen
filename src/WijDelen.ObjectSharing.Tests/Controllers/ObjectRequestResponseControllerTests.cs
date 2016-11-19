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
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ObjectRequestResponseControllerTests {
        [Test]
        public void WhenGettingDenyForUnknownId_ShouldReturnNotFound() {
            var id = Guid.NewGuid();

            var persistentRecords = new ObjectRequestRecord[0];

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, null, null, null);

            var actionResult = controller.Deny(id);

            actionResult.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void WhenGettingDeny_ShouldPersistNoAndReturnView() {
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

            DenyObjectRequest denyObjectRequest = null;
            var commandHandlerMock = new Mock<ICommandHandler<DenyObjectRequest>>();
            commandHandlerMock.Setup(x => x.Handle(It.IsAny<DenyObjectRequest>())).Callback((DenyObjectRequest e) => denyObjectRequest = e);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, services, null, commandHandlerMock.Object, null);

            var actionResult = controller.Deny(id);

            actionResult.Should().BeOfType<ViewResult>();
            denyObjectRequest.DenyingUserId.Should().Be(22);
            denyObjectRequest.ObjectRequestId.Should().Be(id);
        }

        [Test]
        public void WhenGettingConfirmForUnknownId_ShouldReturnNotFound() {
            var id = Guid.NewGuid();

            var persistentRecords = new ObjectRequestRecord[0];

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, null, null, null);

            var actionResult = controller.Deny(id);

            actionResult.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void WhenGettingConfirm_ShouldCallCommandHandlerAndReturnView() {
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

            ConfirmObjectRequest confirmObjectRequest = null;
            var commandHandlerMock = new Mock<ICommandHandler<ConfirmObjectRequest>>();
            commandHandlerMock.Setup(x => x.Handle(It.IsAny<ConfirmObjectRequest>())).Callback((ConfirmObjectRequest e) => confirmObjectRequest = e);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, services, commandHandlerMock.Object, null, null);

            var actionResult = controller.Confirm(id);

            actionResult.Should().BeOfType<ViewResult>();
            confirmObjectRequest.ConfirmingUserId.Should().Be(22);
            confirmObjectRequest.ObjectRequestId.Should().Be(id);
        }

        [Test]
        public void WhenGettingDenyForNowForUnknownId_ShouldReturnNotFound() {
            var id = Guid.NewGuid();

            var persistentRecords = new ObjectRequestRecord[0];

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, null, null, null);

            var actionResult = controller.DenyForNow(id);

            actionResult.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void WhenGettingDenyForNow_ShouldPersistNoAndReturnView() {
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

            DenyObjectRequestForNow denyObjectRequestForNow = null;
            var commandHandlerMock = new Mock<ICommandHandler<DenyObjectRequestForNow>>();
            commandHandlerMock.Setup(x => x.Handle(It.IsAny<DenyObjectRequestForNow>())).Callback((DenyObjectRequestForNow e) => denyObjectRequestForNow = e);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, services, null, null, commandHandlerMock.Object);

            var actionResult = controller.DenyForNow(id);

            actionResult.Should().BeOfType<ViewResult>();
            denyObjectRequestForNow.DenyingUserId.Should().Be(22);
            denyObjectRequestForNow.ObjectRequestId.Should().Be(id);
        }
    }
}