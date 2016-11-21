using System;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ChatControllerTests {
        [Test]
        public void WhenGettingChat() {
            var chatId = Guid.NewGuid();
            var repositoryMock = new Mock<IRepository<ChatMessageRecord>>();
            repositoryMock.SetRecords(new [] {
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

            var controller = new ChatController(repositoryMock.Object);

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
    }
}