using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ChatStarterTests {
        [Test]
        public void WhenObjectRequestConfirmed_ShouldStartChat() {
            var objectRequestId = Guid.NewGuid();
            var objectRequestConfirmed = new ObjectRequestConfirmed {
                SourceId = objectRequestId,
                ConfirmingUserId = 22
            };

            var objectRequestRecord = new ObjectRequestRecord {
                AggregateId = objectRequestId,
                UserId = 123
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new [] {
                objectRequestRecord
            });

            Chat persistedChat = null;
            var repositoryMock = new Mock<IEventSourcedRepository<Chat>>();
            repositoryMock.Setup(x => x.Save(It.IsAny<Chat>(), It.IsAny<string>())).Callback((Chat c, string correlationId) => { persistedChat = c; });

            var chatStarter = new ChatStarter(repositoryMock.Object, objectRequestRepositoryMock.Object);


            chatStarter.Handle(objectRequestConfirmed);

            persistedChat.ObjectRequestId.Should().Be(objectRequestId);
            persistedChat.ConfirmingUserId.Should().Be(22);
            persistedChat.RequestingUserId.Should().Be(123);
        }
    }
}