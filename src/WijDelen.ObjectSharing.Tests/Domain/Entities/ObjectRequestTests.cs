using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.ValueTypes;

namespace WijDelen.ObjectSharing.Tests.Domain.Entities {
    [TestFixture]
    public class ObjectRequestTests {
        [Test]
        public void WhenCreatingObjectRequest() {
            var id = Guid.NewGuid();
            var objectRequest = new ObjectRequest(id, "Sneakers", "for sneaking", 22);

            objectRequest.Description.Should().Be("Sneakers");
            objectRequest.ExtraInfo.Should().Be("for sneaking");

            objectRequest.Events.Single().ShouldBeEquivalentTo(new ObjectRequested {
                SourceId = id,
                Description = "Sneakers",
                ExtraInfo = "for sneaking",
                UserId = 22
            });

            ((ObjectRequested) objectRequest.Events.Single()).CreatedDateTime.Should().NotBe(default(DateTime));
            ((ObjectRequested) objectRequest.Events.Single()).CreatedDateTime.Kind.Should().Be(DateTimeKind.Utc);
            ((ObjectRequested) objectRequest.Events.Single()).Status.Should().Be(ObjectRequestStatus.None);

            objectRequest.Version.Should().Be(0);
            objectRequest.Status.Should().Be(ObjectRequestStatus.None);
        }

        [Test]
        public void WhenCreatingObjectRequestWithForbiddenWords()
        {
            var id = Guid.NewGuid();
            var objectRequest = new ObjectRequest(id, "Sex", "for Sexing", 22);

            objectRequest.Description.Should().Be("Sex");
            objectRequest.ExtraInfo.Should().Be("for Sexing");

            objectRequest.Events.First().ShouldBeEquivalentTo(new ObjectRequested {
                SourceId = id,
                Description = "Sex",
                ExtraInfo = "for Sexing",
                UserId = 22,
                Status = ObjectRequestStatus.BlockedForForbiddenWords,
                Version = 0
            });

            objectRequest.Events.Last().ShouldBeEquivalentTo(new ObjectRequestBlocked
            {
                SourceId = id,
                Description = "Sex",
                ExtraInfo = "for Sexing",
                UserId = 22,
                Version = 1
            });

            ((ObjectRequestBlocked)objectRequest.Events.Last()).ForbiddenWords.ShouldBeEquivalentTo(new List<string> { "sex" });

            var events = objectRequest.Events.ToList();
            events.Count.Should().Be(2);

            ((ObjectRequested)events[0]).CreatedDateTime.Should().NotBe(default(DateTime));
            ((ObjectRequested)events[0]).CreatedDateTime.Kind.Should().Be(DateTimeKind.Utc);

            objectRequest.Version.Should().Be(1);
            objectRequest.Status.Should().Be(ObjectRequestStatus.BlockedForForbiddenWords);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var previousEvent = new ObjectRequested {
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                UserId = 22,
                CreatedDateTime = new DateTime(2016, 12, 27)
            };

            var objectRequest = new ObjectRequest(id, new [] {previousEvent});

            objectRequest.Events.Should().BeEmpty();
            objectRequest.Version.Should().Be(0);
            objectRequest.Description.Should().Be("Sneakers");
            objectRequest.ExtraInfo.Should().Be("For sneaking");
            objectRequest.UserId.Should().Be(22);
            objectRequest.CreatedDateTime.Should().Be(new DateTime(2016, 12, 27));
            objectRequest.Status.Should().Be(ObjectRequestStatus.None);
        }

        [Test]
        public void WhenConfirming() {
            var objectRequest = new ObjectRequest(Guid.NewGuid(), "Sneakers", "For sneaking", 22);

            objectRequest.Confirm(3);

            objectRequest.Events.Last().As<ObjectRequestConfirmed>().ConfirmingUserId.Should().Be(3);
            objectRequest.Events.Last().As<ObjectRequestConfirmed>().DateTimeConfirmed.Should().NotBe(default(DateTime));
            objectRequest.Version.Should().Be(1);
            objectRequest.ConfirmingUserIds.ShouldBeEquivalentTo(new List<int> { 3 });
        }

        [Test]
        public void WhenDenying() {
            var objectRequest = new ObjectRequest(Guid.NewGuid(), "Sneakers", "For sneaking", 22);

            objectRequest.Deny(3);

            objectRequest.Events.Last().As<ObjectRequestDenied>().DenyingUserId.Should().Be(3);
            objectRequest.Events.Last().As<ObjectRequestDenied>().DateTimeDenied.Should().NotBe(default(DateTime));
            objectRequest.Version.Should().Be(1);
            objectRequest.DenyingUserIds.ShouldBeEquivalentTo(new List<int> { 3 });
        }

        [Test]
        public void WhenDenyingForNow() {
            var objectRequest = new ObjectRequest(Guid.NewGuid(), "Sneakers", "For sneaking", 22);

            objectRequest.DenyForNow(3);

            objectRequest.Events.Last().As<ObjectRequestDeniedForNow>().DenyingUserId.Should().Be(3);
            objectRequest.Events.Last().As<ObjectRequestDeniedForNow>().DateTimeDenied.Should().NotBe(default(DateTime));
            objectRequest.Version.Should().Be(1);
            objectRequest.DenyingForNowUserIds.ShouldBeEquivalentTo(new List<int> { 3 });
        }

        [Test]
        public void WhenUnblocking() {
            var objectRequest = new ObjectRequest(Guid.NewGuid(), "Sex", "for sextanting", 22);
            objectRequest.Status.Should().Be(ObjectRequestStatus.BlockedForForbiddenWords);

            objectRequest.Unblock();

            objectRequest.Events.Last().As<ObjectRequestUnblocked>().Description.Should().Be("Sex");
            objectRequest.Events.Last().As<ObjectRequestUnblocked>().ExtraInfo.Should().Be("for sextanting");
            objectRequest.Events.Last().As<ObjectRequestUnblocked>().UserId.Should().Be(22);
            objectRequest.Version.Should().Be(2);
            objectRequest.Status.Should().Be(ObjectRequestStatus.None);
        }

        [Test]
        public void WhenUnblockingManuallyBlockedRequest() {
            var objectRequest = new ObjectRequest(Guid.NewGuid(), "Sneakers", "for sneaking", 22);
            objectRequest.Block("Enough with the sneakers already");
            objectRequest.Status.Should().Be(ObjectRequestStatus.BlockedByAdmin);
            objectRequest.BlockReason.Should().Be("Enough with the sneakers already");

            objectRequest.Unblock();

            objectRequest.Events.Last().As<ObjectRequestUnblocked>().Description.Should().Be("Sneakers");
            objectRequest.Events.Last().As<ObjectRequestUnblocked>().ExtraInfo.Should().Be("for sneaking");
            objectRequest.Events.Last().As<ObjectRequestUnblocked>().UserId.Should().Be(22);
            objectRequest.Version.Should().Be(2);
            objectRequest.Status.Should().Be(ObjectRequestStatus.None);
            objectRequest.BlockReason.Should().BeEmpty();
        }

        [Test]
        public void WhenBlocking() {
            var objectRequest = new ObjectRequest(Guid.NewGuid(), "Sneakers", "for sneaking", 22);
            objectRequest.Status.Should().Be(ObjectRequestStatus.None);

            objectRequest.Block("Just because");

            objectRequest.Events.Last().As<ObjectRequestBlockedByAdmin>().Reason.Should().Be("Just because");
            objectRequest.Version.Should().Be(1);
            objectRequest.Status.Should().Be(ObjectRequestStatus.BlockedByAdmin);
            objectRequest.BlockReason.Should().Be("Just because");
        }
    }
}