using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.ValueTypes;

namespace WijDelen.ObjectSharing.Tests.Domain.Entities {
    [TestFixture]
    public class ObjectRequestedNotificationTests {
        [Test]
        public void Send_ShouldAddSendRequestedEvent() {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var objectRequestedNotification = new ObjectRequestedNotification(
                id,
                22,
                66,
                "Sneakers",
                "For sneaking",
                ObjectRequestStatus.BlockedForForbiddenWords,
                objectRequestId);

            objectRequestedNotification.Send();

            objectRequestedNotification.Events.Last().Should().BeOfType<SendObjectRequestedNotificationRequested>();
            objectRequestedNotification.Events.Last().ShouldBeEquivalentTo(new SendObjectRequestedNotificationRequested {
                SourceId = id,
                RequestingUserId = 22,
                ReceivingUserId = 66,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                ObjectRequestId = objectRequestId,
                Version = 1,
                Status = ObjectRequestStatus.BlockedForForbiddenWords
            });
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var previousEvent = new ObjectRequestedNotificationCreated {
                RequestingUserId = 22,
                ReceivingUserId = 66,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                ObjectRequestId = objectRequestId,
                Status = ObjectRequestStatus.BlockedForForbiddenWords
            };

            var objectRequestedNotification = new ObjectRequestedNotification(id, new[] {previousEvent});

            objectRequestedNotification.Events.Should().BeEmpty();
            objectRequestedNotification.Version.Should().Be(0);
            objectRequestedNotification.Id.Should().Be(id);
            objectRequestedNotification.RequestingUserId.Should().Be(22);
            objectRequestedNotification.ReceivingUserId.Should().Be(66);
            objectRequestedNotification.Description.Should().Be("Sneakers");
            objectRequestedNotification.ExtraInfo.Should().Be("For sneaking");
            objectRequestedNotification.ObjectRequestId.Should().Be(objectRequestId);
            objectRequestedNotification.Status.Should().Be(ObjectRequestStatus.BlockedForForbiddenWords);
        }

        [Test]
        public void MarkAsSent_ShouldAddSentEvent() {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var objectRequestedNotification = new ObjectRequestedNotification(
                id,
                22,
                66,
                "Sneakers",
                "For sneaking",
                ObjectRequestStatus.BlockedForForbiddenWords,
                objectRequestId);

            objectRequestedNotification.MarkAsSent();

            objectRequestedNotification.Events.Last().Should().BeOfType<ObjectRequestedNotificationSent>();
            objectRequestedNotification.Events.Last().ShouldBeEquivalentTo(new ObjectRequestedNotificationSent
            {
                SourceId = id,
                ObjectRequestId = objectRequestId,
                Version = 1
            });
        }

        [Test]
        public void WhenCreatingObjectRequestedNotification() {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var objectRequestedNotification = new ObjectRequestedNotification(
                id,
                22,
                66,
                "Sneakers",
                "For sneaking",
                ObjectRequestStatus.BlockedForForbiddenWords,
                objectRequestId);

            objectRequestedNotification.Id.Should().Be(id);
            objectRequestedNotification.RequestingUserId.Should().Be(22);
            objectRequestedNotification.Description.Should().Be("Sneakers");
            objectRequestedNotification.ExtraInfo.Should().Be("For sneaking");

            objectRequestedNotification.Events.Single().ShouldBeEquivalentTo(new ObjectRequestedNotificationCreated {
                SourceId = id,
                RequestingUserId = 22,
                ReceivingUserId = 66,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                ObjectRequestId = objectRequestId,
                Status = ObjectRequestStatus.BlockedForForbiddenWords
            });

            objectRequestedNotification.Version.Should().Be(0);
            objectRequestedNotification.ObjectRequestId.Should().Be(objectRequestId);
        }
    }
}