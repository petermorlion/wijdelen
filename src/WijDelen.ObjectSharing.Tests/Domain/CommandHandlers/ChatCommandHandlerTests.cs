using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.CommandHandlers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Infrastructure;

namespace WijDelen.ObjectSharing.Tests.Domain.CommandHandlers {
    [TestFixture]
    public class ChatCommandHandlerTests {
        [Test]
        public void WhenAddingChatMessage() {
            var chat = new Chat(Guid.NewGuid(), Guid.NewGuid(), 1, 2);
            var command = new AddChatMessage(chat.Id, 2, "Hello", new DateTime(2016, 11, 22));

            var repositoryMock = new Mock<IEventSourcedRepository<Chat>>();
            repositoryMock
                .Setup(x => x.Save(It.IsAny<Chat>(), command.Id.ToString()))
                .Callback((Chat c, string ci) => chat = c);
            repositoryMock
                .Setup(x => x.Find(chat.Id))
                .Returns(chat);

            var mailServiceMock = new Mock<IMailService>();

            var handler = new ChatCommandHandler(repositoryMock.Object);

            handler.Handle(command);

            chat.Messages.Single().UserId.Should().Be(2);
            chat.Messages.Single().Message.Should().Be("Hello");
            chat.Messages.Single().DateTime.Should().Be(new DateTime(2016, 11, 22));
        }
    }
}