using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.CommandHandlers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Tests.Domain.CommandHandlers {
    [TestFixture]
    public class ChatCommandHandlerTests {
        [Test]
        public void WhenHandlingStartChat_ShouldCreateNewChat() {
            Chat chat = null;
            var startChat = new StartChat(Guid.NewGuid(), 1, 2);
            var repositoryMock = new Mock<IEventSourcedRepository<Chat>>();
            repositoryMock
                .Setup(x => x.Save(It.IsAny<Chat>(), startChat.Id.ToString()))
                .Callback((Chat c, string ci) => chat = c);

            var handler = new ChatCommandHandler(repositoryMock.Object);

            handler.Handle(startChat);

            chat.ConfirmingUserId.Should().Be(2);
            chat.ObjectRequestId.Should().Be(startChat.ObjectRequestId);
            chat.RequestingUserId.Should().Be(1);
        }
    }
}