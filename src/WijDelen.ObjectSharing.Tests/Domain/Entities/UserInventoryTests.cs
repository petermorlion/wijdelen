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
            var userInventory = new UserInventory(id, 22);

            userInventory.UserId.Should().Be(22);

            userInventory.Events.Single().ShouldBeEquivalentTo(new UserInventoryCreated {
                SourceId = id,
                UserId = 22
            });

            userInventory.Version.Should().Be(0);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var previousEvent = new UserInventoryCreated {
                UserId = 22
            };

            var userInventory = new UserInventory(id, new [] {previousEvent});

            userInventory.Events.Should().BeEmpty();
            userInventory.Version.Should().Be(0);
            userInventory.UserId.Should().Be(22);
        }

        [Test]
        public void WhenMarkingAsOwned() {
            var userInventory = new UserInventory(Guid.NewGuid(), 22);

            userInventory.MarkAsOwned(1, "Sneakers");

            userInventory.Version.Should().Be(1);
            userInventory.Events.OfType<ArchetypeMarkedAsOwned>().Single().UserId.Should().Be(22);
            userInventory.Events.OfType<ArchetypeMarkedAsOwned>().Single().ArchetypeId.Should().Be(1);
            userInventory.Events.OfType<ArchetypeMarkedAsOwned>().Single().ArchetypeTitle.Should().Be("Sneakers");
            userInventory.OwnedArchetypeIds.ShouldBeEquivalentTo(new[] {1});
        }

        [Test]
        public void WhenMarkingAsNotOwned() {
            var userInventory = new UserInventory(Guid.NewGuid(), 22);

            userInventory.MarkAsNotOwned(1, "Sneakers");

            userInventory.Version.Should().Be(1);
            userInventory.Events.OfType<ArchetypeMarkedAsNotOwned>().Single().UserId.Should().Be(22);
            userInventory.Events.OfType<ArchetypeMarkedAsNotOwned>().Single().ArchetypeId.Should().Be(1);
            userInventory.Events.OfType<ArchetypeMarkedAsNotOwned>().Single().ArchetypeTitle.Should().Be("Sneakers");
            userInventory.NotOwnedArchetypeIds.ShouldBeEquivalentTo(new[] {1});
        }

        [Test]
        public void WhenMarkingAsOwnedAndNotOwned() {
            var userInventory = new UserInventory(Guid.NewGuid(), 22);

            userInventory.MarkAsOwned(2, "Reebok pumps");
            userInventory.MarkAsNotOwned(1, "Sneakers");
            userInventory.MarkAsNotOwned(2, "Reebok pumps");
            userInventory.MarkAsOwned(1, "Sneakers");

            userInventory.Version.Should().Be(4);
            userInventory.NotOwnedArchetypeIds.ShouldBeEquivalentTo(new[] {2});
            userInventory.OwnedArchetypeIds.ShouldBeEquivalentTo(new[] {1});
        }
    }
}