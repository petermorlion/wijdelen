using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;

namespace WijDelen.ObjectSharing.Tests.Domain.Entities {
    [TestFixture]
    public class ItemArchetypeTests {
        [Test]
        public void WhenCreatingItemArchetype()
        {
            var id = Guid.NewGuid();
            var itemArchetype = new ItemArchetype(id, "Sneakers");

            itemArchetype.Name.Should().Be("Sneakers");

            itemArchetype.Events.Single().ShouldBeEquivalentTo(new ItemArchetypeCreated
            {
                SourceId = id,
                Name = "Sneakers"
            });

            itemArchetype.Version.Should().Be(0);
        }
    }
}