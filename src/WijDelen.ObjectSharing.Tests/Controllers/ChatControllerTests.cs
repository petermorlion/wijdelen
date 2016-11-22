using System;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Data;
using Orchard.Security;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventSourcing;
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
            var repositoryMock = new Mock<IRepository<ChatMessageRecord>>();
            repositoryMock.SetRecords(new[] {
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

            var controller = new ChatController(
                repositoryMock.Object, 
                default(IRepository<ObjectRequestRecord>), 
                default(ICommandHandler<StartChat>),
                default(ICommandHandler<AddChatMessage>),
                default(IEventSourcedRepository<Chat>),
                default(IOrchardServices));

            var result = controller.Index(chatId);

            result.Should().BeOfType<ViewResult>();
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
        public void WhenStartingNewChat() {
            var objectRequestId = Guid.NewGuid();

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid()
                },
                new ObjectRequestRecord {
                    AggregateId = objectRequestId,
                    UserId = 22
                }
            });

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(23);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;

            StartChat command = null;
            var commandHandlerMock = new Mock<ICommandHandler<StartChat>>();
            commandHandlerMock
                .Setup(x => x.Handle(It.IsAny<StartChat>()))
                .Callback((StartChat c) => command = c);

            var controller = new ChatController(
                default(IRepository<ChatMessageRecord>), 
                objectRequestRepositoryMock.Object, 
                commandHandlerMock.Object, 
                default(ICommandHandler<AddChatMessage>),
                default(IEventSourcedRepository<Chat>),
                services);

            var result = controller.Start(objectRequestId);

            result.Should().BeOfType<RedirectToRouteResult>();
            result.As<RedirectToRouteResult>().RouteValues["action"].Should().Be("Index");
            result.As<RedirectToRouteResult>().RouteValues["chatId"].Should().Be(command.ChatId);
        }

        [Test]
        public void WhenStartingNewChatForUnknownObjectRequest() {
            var objectRequestId = Guid.NewGuid();

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid()
                }
            });

            var controller = new ChatController(
                default(IRepository<ChatMessageRecord>), 
                objectRequestRepositoryMock.Object, 
                default(ICommandHandler<StartChat>), 
                default(ICommandHandler<AddChatMessage>),
                default(IEventSourcedRepository<Chat>),
                default(IOrchardServices));

            var result = controller.Start(objectRequestId);

            result.Should().BeOfType<HttpNotFoundResult>();
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
                default(ICommandHandler<StartChat>),
                default(ICommandHandler<AddChatMessage>),
                default(IEventSourcedRepository<Chat>),
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

            var chatRepositoryMock = new Mock<IEventSourcedRepository<Chat>>();
            chatRepositoryMock.Setup(x => x.Find(chatId)).Returns(new Chat(chatId, Guid.NewGuid(), 1, 23));

            var controller = new ChatController(
                default(IRepository<ChatMessageRecord>),
                default(IRepository<ObjectRequestRecord>),
                default(ICommandHandler<StartChat>),
                commandHandlerMock.Object,
                chatRepositoryMock.Object,
                services);

            controller.AddMessage(chatId, "Hello");

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

            var chatRepositoryMock = new Mock<IEventSourcedRepository<Chat>>();
            chatRepositoryMock.Setup(x => x.Find(chatId)).Returns(new Chat(chatId, Guid.NewGuid(), 1, 2));

            var controller = new ChatController(
                default(IRepository<ChatMessageRecord>),
                default(IRepository<ObjectRequestRecord>),
                default(ICommandHandler<StartChat>),
                default(ICommandHandler<AddChatMessage>),
                chatRepositoryMock.Object,
                services);

            var result = controller.AddMessage(chatId, "Hello");

            result.Should().BeOfType<HttpUnauthorizedResult>();
        }
    }
}