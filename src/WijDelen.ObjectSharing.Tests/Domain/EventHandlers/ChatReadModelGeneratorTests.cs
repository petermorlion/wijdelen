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
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ChatReadModelGeneratorTests {
        [Test]
        public void WhenHandlingChatStarted() {
            var chatStarted = new ChatStarted {
                SourceId = Guid.NewGuid(),
                ConfirmingUserId = 1,
                ObjectRequestId = Guid.NewGuid(),
                RequestingUserId = 2
            };

            ChatRecord record = null;
            var repositoryMock = new Mock<IRepository<ChatRecord>>();
            repositoryMock
                .Setup(x => x.Create(It.IsAny<ChatRecord>()))
                .Callback((ChatRecord r) => record = r);

            var fakeUserFactory = new UserFactory();
            var confirmingUserMock = fakeUserFactory.Create("Carl", "carl@thesimpsons.com", "Carlton", "Carlson");
            var requestingUserMock = fakeUserFactory.Create("Lenny", "lenny@thesimpsons.com", "Lenford", "Leonard");
            var userQueryMock = new Mock<IGetUserByIdQuery>();
            userQueryMock.Setup(x => x.GetResult(1)).Returns(confirmingUserMock);
            userQueryMock.Setup(x => x.GetResult(2)).Returns(requestingUserMock);

            var handler = new ChatReadModelGenerator(repositoryMock.Object, userQueryMock.Object);

            handler.Handle(chatStarted);

            record.ChatId.Should().Be(chatStarted.SourceId);
            record.ObjectRequestId.Should().Be(chatStarted.ObjectRequestId);
            record.ConfirmingUserId.Should().Be(1);
            record.ConfirmingUserName.Should().Be("Carlton Carlson");
            record.RequestingUserId.Should().Be(2);
            record.RequestingUserName.Should().Be("Lenford Leonard");
        }
    }
}