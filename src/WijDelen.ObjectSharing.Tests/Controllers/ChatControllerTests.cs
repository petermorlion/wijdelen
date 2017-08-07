using System;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ChatControllerTests {
        private ChatController _controller;
        private Guid _chatId;
        private ObjectRequestRecord _objectRequest;
        private Mock<INotifier> _notifierMock;
        private Mock<ICommandHandler<AddChatMessage>> _addChatMessageCommandHandlerMock;
        private Mock<IUser> _userMock;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();
            var chatMessageRepositoryMock = new Mock<IRepository<ChatMessageRecord>>();
            _addChatMessageCommandHandlerMock = new Mock<ICommandHandler<AddChatMessage>>();

            _notifierMock = new Mock<INotifier>();
            _userMock = new Mock<IUser>();
            _userMock.Setup(x => x.Id).Returns(666);
            var fakeOrchardServices = new FakeOrchardServices();
            fakeOrchardServices.WorkContext.CurrentUser = _userMock.Object;
            fakeOrchardServices.Notifier = _notifierMock.Object;

            builder.RegisterInstance(objectRequestRepositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            builder.RegisterInstance(chatRepositoryMock.Object).As<IRepository<ChatRecord>>();
            builder.RegisterInstance(chatMessageRepositoryMock.Object).As<IRepository<ChatMessageRecord>>();
            builder.RegisterInstance(_addChatMessageCommandHandlerMock.Object).As<ICommandHandler<AddChatMessage>>();
            builder.RegisterInstance(fakeOrchardServices).As<IOrchardServices>();
            builder.RegisterType<ChatController>();

            var objectRequestId = Guid.NewGuid();
            _objectRequest = new ObjectRequestRecord
            {
                AggregateId = objectRequestId,
                Description = "Sneakers",
                Status = ObjectRequestStatus.None.ToString()
            };

            objectRequestRepositoryMock.SetRecords(new[] { _objectRequest });

            _chatId = Guid.NewGuid();
            var chatRecord = new ChatRecord
            {
                ObjectRequestId = objectRequestId,
                ChatId = _chatId,
                RequestingUserName = "Carl",
                RequestingUserId = 666,
                ConfirmingUserId = 2
            };

            chatRepositoryMock.SetRecords(new[] { chatRecord });

            chatMessageRepositoryMock.SetRecords(new[] {
                new ChatMessageRecord {
                    ChatId = _chatId,
                    DateTime = new DateTime(2016, 11, 22),
                    UserName = "Moe",
                    Message = "Hello",
                    UserId = 2
                },
                new ChatMessageRecord {
                    ChatId = _chatId,
                    DateTime = new DateTime(2016, 11, 21),
                    UserName = "Lenny",
                    Message = "Hi",
                    UserId = 1
                },
                new ChatMessageRecord {
                    ChatId = _chatId,
                    DateTime = new DateTime(2016, 11, 23),
                    UserName = "Moe",
                    Message = "How are you?",
                    UserId = 2
                },
                new ChatMessageRecord {
                    ChatId = Guid.NewGuid(),
                    DateTime = new DateTime(2016, 11, 22),
                    UserName = "Carl",
                    Message = "Howdy",
                    UserId = 3
                }
            });

            var container = builder.Build();
            _controller = container.Resolve<ChatController>();

            _controller.T = NullLocalizer.Instance;
        }

        [Test]
        public void WhenGettingExistingChat() {
            var result = _controller.Index(_chatId);

            result.Should().BeOfType<ViewResult>();
            ViewResultShouldContainAllChatData(result);
            result.As<ViewResult>().Model.As<ChatViewModel>().IsForBlockedObjectRequest.Should().BeFalse();
        }

        [Test]
        public void WhenGettingExistingChatForBlockedRequest() {
            _objectRequest.Status = ObjectRequestStatus.BlockedByAdmin.ToString();

            var result = _controller.Index(_chatId);

            result.Should().BeOfType<ViewResult>();
            ViewResultShouldContainAllChatData(result);
            _notifierMock.Verify(x => x.Add(NotifyType.Warning, new LocalizedString("This request is blocked. It is currently not possible to add new messages.")));
            result.As<ViewResult>().Model.As<ChatViewModel>().IsForBlockedObjectRequest.Should().BeTrue();
        }

        [Test]
        public void WhenGettingUnknownChat() {
            var chatId = Guid.NewGuid();

            var result = _controller.Index(chatId);

            result.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void WhenAddingChatMessage() {
            AddChatMessage command = null;

            _addChatMessageCommandHandlerMock
                .Setup(x => x.Handle(It.IsAny<AddChatMessage>()))
                .Callback((AddChatMessage c) => command = c);
            
            var result = _controller.Index(new ChatViewModel {ChatId = _chatId, NewMessage = "Hello"});

            result.Should().BeOfType<RedirectToRouteResult>();
            result.As<RedirectToRouteResult>().RouteValues["action"].Should().Be("Index");
            result.As<RedirectToRouteResult>().RouteValues["id"].Should().Be(_chatId);
            command.ChatId.Should().Be(_chatId);
            command.UserId.Should().Be(666);
            command.Message.Should().Be("Hello");
            command.DateTime.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public void WhenAddingChatMessageForOtherUser() {
            _userMock.Setup(x => x.Id).Returns(1);

            var result = _controller.Index(new ChatViewModel {ChatId = _chatId, NewMessage = "Hello"});

            result.Should().BeOfType<HttpUnauthorizedResult>();
        }

        [Test]
        public void WhenAddingEmptyChatMessage() {
            var result = _controller.Index(new ChatViewModel {ChatId = _chatId, NewMessage = ""});

            result.Should().BeOfType<ViewResult>();
            ViewResultShouldContainAllChatData(result);
            result.As<ViewResult>().ViewName.Should().Be("");
            result.As<ViewResult>().ViewData.ModelState.Values.ToList()[0].Errors.ToList()[0].ErrorMessage.Should().Be("Please provide a message.");
        }

        [Test]
        public void WhenGettingExistingChatForStoppedRequest() {
            _objectRequest.Status = ObjectRequestStatus.Stopped.ToString();

            var result = _controller.Index(_chatId);

            result.Should().BeOfType<ViewResult>();
            ViewResultShouldContainAllChatData(result);
            _notifierMock.Verify(x => x.Add(NotifyType.Warning, new LocalizedString("This request has been stopped by Carl. It is currently not possible to add new messages.")));
            result.As<ViewResult>().Model.As<ChatViewModel>().IsStopped.Should().BeTrue();
        }

        private void ViewResultShouldContainAllChatData(ActionResult result) {
            result.As<ViewResult>().Model.As<ChatViewModel>().ChatId.Should().Be(_chatId);
            result.As<ViewResult>().Model.As<ChatViewModel>().ObjectDescription.Should().Be("Sneakers");
            result.As<ViewResult>().Model.As<ChatViewModel>().RequestingUserName.Should().Be("Carl");
            result.As<ViewResult>().Model.As<ChatViewModel>().RequestingUserId.Should().Be(666);
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages.Count.Should().Be(3);
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[0].DateTime.Should().Be(new DateTime(2016, 11, 21));
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[0].UserId.Should().Be(1);
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[0].Message.Should().Be("Hi");
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[1].DateTime.Should().Be(new DateTime(2016, 11, 22));
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[1].UserId.Should().Be(2);
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[1].Message.Should().Be("Hello");
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[2].DateTime.Should().Be(new DateTime(2016, 11, 23));
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[2].UserId.Should().Be(2);
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[2].Message.Should().Be("How are you?");
        }
    }
}