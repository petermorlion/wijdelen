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

            var chatMessageRepositoryMock = new Mock<IRepository<ChatMessageRecord>>();
            chatMessageRepositoryMock.SetRecords(new[] {
                new ChatMessageRecord {
                    ChatId = Guid.NewGuid()
                }
            });

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

            var controller = new ChatController(chatMessageRepositoryMock.Object, objectRequestRepositoryMock.Object, commandHandlerMock.Object, services);
            var result = controller.Start(objectRequestId);

            result.Should().BeOfType<ViewResult>();
            command.ObjectRequestId.Should().Be(objectRequestId);
            command.RequestingUserId.Should().Be(22);
            command.ConfirmingUserId.Should().Be(23);
        }

        [Test]
        public void WhenGettingNewChatForUnknownObjectRequest() {
            // TODO
        }

        [Test]
        public void WhenGettingNewChatForOtherUser() {
            // TODO
        }

        [Test]
        public void WhenGettingUnknownChat() {
            // TODO
        }

        [Test]
        public void WhenAddingChatMessage() {
            // TODO
        }

        [Test]
        public void WhenAddingChatMessageForOtherUser() {
            // TODO
        }
    }
}