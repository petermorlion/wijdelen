using System;
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

            objectRequest.Version.Should().Be(0);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var previousEvent = new ObjectRequested {
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                UserId = 22
            };

            var objectRequest = new ObjectRequest(id, new [] {previousEvent});

            objectRequest.Events.Should().BeEmpty();
            objectRequest.Version.Should().Be(0);
            objectRequest.Description.Should().Be("Sneakers");
            objectRequest.ExtraInfo.Should().Be("For sneaking");
            objectRequest.UserId.Should().Be(22);
        }
    }
}