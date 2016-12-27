using System;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ChatControllerTests {
        [Test]
        public void WhenGettingExistingChat() {
            var chatId = Guid.NewGuid();
            var chatMessageRepositoryMock = new Mock<IRepository<ChatMessageRecord>>();
            chatMessageRepositoryMock.SetRecords(new[] {
                new ChatMessageRecord {
                    ChatId = chatId,
                    DateTime = new DateTime(2016, 11, 22),
                    UserName = "Moe",
                    Message = "Hello"
                },
                new ChatMessageRecord {
                    ChatId = chatId,
                    DateTime = new DateTime(2016, 11, 21),
                    UserName = "Lenny",
                    Message = "Hi"
                },
                new ChatMessageRecord {
                    ChatId = chatId,
                    DateTime = new DateTime(2016, 11, 23),
                    UserName = "Moe",
                    Message = "How are you?"
                },
                new ChatMessageRecord {
                    ChatId = Guid.NewGuid(),
                    DateTime = new DateTime(2016, 11, 22),
                    UserName = "Carl",
                    Message = "Howdy"
                }
            });

            var objectRequestId = Guid.NewGuid();
            var objectRequest = new ObjectRequestRecord {
                AggregateId = objectRequestId,
                Description = "Sneakers"
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] { objectRequest });

            var chatRecord = new ChatRecord {
                ObjectRequestId = objectRequestId,
                ChatId = chatId,
                RequestingUserName = "Carl"
            };

            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();
            chatRepositoryMock.SetRecords(new[] {chatRecord});

            var controller = new ChatController(
                chatMessageRepositoryMock.Object,
                objectRequestRepositoryMock.Object,
                default(ICommandHandler<AddChatMessage>),
                chatRepositoryMock.Object,
                default(IOrchardServices));

            var result = controller.Index(chatId);

            result.Should().BeOfType<ViewResult>();
            result.As<ViewResult>().Model.As<ChatViewModel>().ChatId.Should().Be(chatId);
            result.As<ViewResult>().Model.As<ChatViewModel>().ObjectDescription.Should().Be("Sneakers");
            result.As<ViewResult>().Model.As<ChatViewModel>().RequestingUserName.Should().Be("Carl");
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages.Count.Should().Be(3);
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[0].DateTime.Should().Be(new DateTime(2016, 11, 21));
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[0].UserName.Should().Be("Lenny");
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[0].Message.Should().Be("Hi");
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[1].DateTime.Should().Be(new DateTime(2016, 11, 22));
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[1].UserName.Should().Be("Moe");
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[1].Message.Should().Be("Hello");
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[2].DateTime.Should().Be(new DateTime(2016, 11, 23));
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[2].UserName.Should().Be("Moe");
            result.As<ViewResult>().Model.As<ChatViewModel>().Messages[2].Message.Should().Be("How are you?");
        }

        [Test]
        public void WhenGettingUnknownChat() {
            var chatId = Guid.NewGuid();
            var repositoryMock = new Mock<IRepository<ChatMessageRecord>>();
            repositoryMock.SetRecords(new[] {
                new ChatMessageRecord {
                    ChatId = Guid.NewGuid(),
                    DateTime = new DateTime(2016, 11, 22),
                    UserName = "Moe",
                    Message = "Hello"
                }
            });

            var controller = new ChatController(
                repositoryMock.Object,
                default(IRepository<ObjectRequestRecord>),
                default(ICommandHandler<AddChatMessage>),
                default(IRepository<ChatRecord>),
                default(IOrchardServices));

            var result = controller.Index(chatId);

            result.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void WhenAddingChatMessage() {
            var chatId = Guid.NewGuid();
            var commandHandlerMock = new Mock<ICommandHandler<AddChatMessage>>();
            AddChatMessage command = null;

            commandHandlerMock
                .Setup(x => x.Handle(It.IsAny<AddChatMessage>()))
                .Callback((AddChatMessage c) => command = c);

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(23);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;

            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();
            var chatRecord = new ChatRecord {
                ChatId = chatId,
                ConfirmingUserId = 2,
                RequestingUserId = 23
            };

            chatRepositoryMock.SetRecords(new[] {chatRecord});

            var controller = new ChatController(
                default(IRepository<ChatMessageRecord>),
                default(IRepository<ObjectRequestRecord>),
                commandHandlerMock.Object,
                chatRepositoryMock.Object,
                services);

            var result = controller.AddMessage(new ChatViewModel {ChatId = chatId, NewMessage = "Hello",});

            result.Should().BeOfType<RedirectToRouteResult>();
            result.As<RedirectToRouteResult>().RouteValues["action"].Should().Be("Index");
            result.As<RedirectToRouteResult>().RouteValues["chatId"].Should().Be(chatId);
            command.ChatId.Should().Be(chatId);
            command.UserId.Should().Be(23);
            command.Message.Should().Be("Hello");
            command.DateTime.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public void WhenAddingChatMessageForOtherUser() {
            var chatId = Guid.NewGuid();

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(23);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;

            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();
            var chatRecord = new ChatRecord {
                ChatId = chatId,
                ConfirmingUserId = 2,
                RequestingUserId = 1
            };

            chatRepositoryMock.SetRecords(new[] {chatRecord});

            var controller = new ChatController(
                default(IRepository<ChatMessageRecord>),
                default(IRepository<ObjectRequestRecord>),
                default(ICommandHandler<AddChatMessage>),
                chatRepositoryMock.Object,
                services);

            var result = controller.AddMessage(new ChatViewModel {ChatId = chatId, NewMessage = "Hello"});

            result.Should().BeOfType<HttpUnauthorizedResult>();
        }

        [Test]
        public void WhenAddingEmptyChatMessage() {
            var chatId = Guid.NewGuid();

            var controller = new ChatController(
                default(IRepository<ChatMessageRecord>),
                default(IRepository<ObjectRequestRecord>),
                default(ICommandHandler<AddChatMessage>),
                default(IRepository<ChatRecord>),
                default(IOrchardServices));

            var result = controller.AddMessage(new ChatViewModel {ChatId = chatId, NewMessage = ""});

            result.Should().BeOfType<ViewResult>();
            result.As<ViewResult>().ViewData.ModelState.Values.ToList()[0].Errors.ToList()[0].ErrorMessage.Should().Be("Please provide a message.");
        }

        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var controller = new ChatController(null, null, null, null, null);
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }
    }
}