using System;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;

namespace WijDelen.ObjectSharing.Tests.Domain.Entities {
    [TestFixture]
    public class UserInventoryTests {
        [Test]
        public void WhenCreatingUserInventory() {
            var id = Guid.NewGuid();
            var objectRequest = new UserInventory(id, 22);

            objectRequest.UserId.Should().Be(22);

            objectRequest.Events.Single().ShouldBeEquivalentTo(new UserInventoryCreated {
                SourceId = id,
                UserId = 22
            });

            objectRequest.Version.Should().Be(0);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var previousEvent = new UserInventoryCreated {
                UserId = 22
            };

            var objectRequest = new UserInventory(id, new [] {previousEvent});

            objectRequest.Events.Should().BeEmpty();
            objectRequest.Version.Should().Be(0);
            objectRequest.UserId.Should().Be(22);
        }
    }
}