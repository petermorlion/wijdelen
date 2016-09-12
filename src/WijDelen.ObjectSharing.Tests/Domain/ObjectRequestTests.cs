using System;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using WijDelen.ObjectSharing.Domain;
using WijDelen.ObjectSharing.Domain.Events;

namespace WijDelen.ObjectSharing.Tests.Domain {
    [TestFixture]
    public class ObjectRequestTests {
        [Test]
        public void WhenCreatingObjectRequest() {
            var id = Guid.NewGuid();
            var objectRequest = new ObjectRequest(id, "Sneakers", "for sneaking");

            objectRequest.Description.Should().Be("Sneakers");
            objectRequest.ExtraInfo.Should().Be("for sneaking");

            objectRequest.Events.Single().ShouldBeEquivalentTo(new ObjectRequested {
                SourceId = id,
                Description = "Sneakers",
                ExtraInfo = "for sneaking"
            });

            objectRequest.Version.Should().Be(0);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var previousEvent = new ObjectRequested {
                Description = "Sneakers",
                ExtraInfo = "For sneaking"
            };

            var objectRequest = new ObjectRequest(id, new [] {previousEvent});

            objectRequest.Events.Should().BeEmpty();
            objectRequest.Version.Should().Be(0);
            objectRequest.Description.Should().Be("Sneakers");
            objectRequest.ExtraInfo.Should().Be("For sneaking");
        }
    }
}