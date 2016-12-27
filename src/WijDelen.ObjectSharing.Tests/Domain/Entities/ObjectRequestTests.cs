using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;

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

            objectRequest.Version.Should().Be(0);
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
        }

        [Test]
        public void WhenConfirming() {
            var objectRequest = new ObjectRequest(Guid.NewGuid(), "Sneakers", "For sneaking", 22);

            objectRequest.Confirm(3);

            objectRequest.Events.Last().As<ObjectRequestConfirmed>().ConfirmingUserId.Should().Be(3);
            objectRequest.Version.Should().Be(1);
            objectRequest.ConfirmingUserIds.ShouldBeEquivalentTo(new List<int> { 3 });
        }

        [Test]
        public void WhenDenying() {
            var objectRequest = new ObjectRequest(Guid.NewGuid(), "Sneakers", "For sneaking", 22);

            objectRequest.Deny(3);

            objectRequest.Events.Last().As<ObjectRequestDenied>().DenyingUserId.Should().Be(3);
            objectRequest.Version.Should().Be(1);
            objectRequest.DenyingUserIds.ShouldBeEquivalentTo(new List<int> { 3 });
        }

        [Test]
        public void WhenDenyingForNow() {
            var objectRequest = new ObjectRequest(Guid.NewGuid(), "Sneakers", "For sneaking", 22);

            objectRequest.DenyForNow(3);

            objectRequest.Events.Last().As<ObjectRequestDeniedForNow>().DenyingUserId.Should().Be(3);
            objectRequest.Version.Should().Be(1);
            objectRequest.DenyingForNowUserIds.ShouldBeEquivalentTo(new List<int> { 3 });
        }
    }
}