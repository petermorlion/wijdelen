using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ChatMessageReadModelGeneratorTests {
        [Test]
        public void WhenChatMessageAdded() {
            var chatMessageAdded = new ChatMessageAdded {
                ChatId = Guid.NewGuid(),
                DateTime = DateTime.Now,
                Message = "Message",
                UserId = 22
            };

            ChatMessageRecord record = null;
            var repositoryMock = new Mock<IRepository<ChatMessageRecord>>();
            repositoryMock
                .Setup(x => x.Create(It.IsAny<ChatMessageRecord>()))
                .Callback((ChatMessageRecord r) => record = r);

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.UserName).Returns("Moe"); 
            var userQueryMock = new Mock<IGetUserByIdQuery>();
            userQueryMock.Setup(x => x.GetResult(22)).Returns(userMock.Object);

            var handler = new ChatMessageReadModelGenerator(repositoryMock.Object, userQueryMock.Object);

            handler.Handle(chatMessageAdded);

            record.UserId.Should().Be(22);
            record.UserName.Should().Be("Moe");
            record.Message.Should().Be("Message");
            record.DateTime.Should().Be(chatMessageAdded.DateTime);
            record.ChatId.Should().Be(chatMessageAdded.ChatId);
        }
    }
}