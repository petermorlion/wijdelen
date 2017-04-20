using System;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
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

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, null, null, null, null);

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

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, services, null, commandHandlerMock.Object, null);

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

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, null, null, null, null);

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
            var notifierMock = new Mock<INotifier>();
            services.Notifier = notifierMock.Object;

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

            var chatId = Guid.NewGuid();
            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();
            chatRepositoryMock.SetRecords(new [] {
                new ChatRecord {
                    ObjectRequestId = id,
                    ConfirmingUserId = 1
                },
                new ChatRecord {
                    ObjectRequestId = Guid.NewGuid(),
                    ConfirmingUserId = 22
                },
                new ChatRecord {
                    ObjectRequestId = id,
                    ConfirmingUserId = 22,
                    ChatId = chatId,
                    RequestingUserName = "John Doe"
                }
            });

            var controller = new ObjectRequestResponseController(repositoryMock.Object, chatRepositoryMock.Object, services, commandHandlerMock.Object, null, null);

            var actionResult = controller.Confirm(id);

            actionResult.Should().BeOfType<RedirectToRouteResult>();
            actionResult.As<RedirectToRouteResult>().RouteValues["action"].Should().Be("Index");
            actionResult.As<RedirectToRouteResult>().RouteValues["controller"].Should().Be("Chat");
            actionResult.As<RedirectToRouteResult>().RouteValues["id"].Should().Be(chatId);
            confirmObjectRequest.ConfirmingUserId.Should().Be(22);
            confirmObjectRequest.ObjectRequestId.Should().Be(id);

            notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Thank you for your response. You can now chat with John Doe.")));
        }

        [Test]
        public void WhenGettingDenyForNowForUnknownId_ShouldReturnNotFound() {
            var id = Guid.NewGuid();

            var persistentRecords = new ObjectRequestRecord[0];

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, null, null, null, null);

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

            var controller = new ObjectRequestResponseController(repositoryMock.Object, null, services, null, null, commandHandlerMock.Object);

            var actionResult = controller.DenyForNow(id);

            actionResult.Should().BeOfType<ViewResult>();
            denyObjectRequestForNow.DenyingUserId.Should().Be(22);
            denyObjectRequestForNow.ObjectRequestId.Should().Be(id);
        }
    }
}