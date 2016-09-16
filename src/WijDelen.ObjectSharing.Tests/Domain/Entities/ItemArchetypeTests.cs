using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Tests.Domain.Entities {
    [TestFixture]
    public class ItemArchetypeTests {
        [Test]
        public void WhenCreatingItemArchetype() {
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

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var created = new ItemArchetypeCreated
            {
                Name = "Sneakers",
                Version = 0
            };

            var synonymAdded = new ItemArchetypeSynonymAdded
            {
                Synonym = "Sport shoes",
                Version = 1
            };

            var itemArchetype = new ItemArchetype(id, new IVersionedEvent[] { created, synonymAdded });

            itemArchetype.Events.Should().BeEmpty();
            itemArchetype.Version.Should().Be(1);
            itemArchetype.Name.Should().Be("Sneakers");
            itemArchetype.Synonyms.Single().Should().Be("Sport shoes");
        }

        [Test]
        public void WhenAddingSynonym() {
            var itemArchetype = new ItemArchetype(Guid.NewGuid(), "Sneakers");

            itemArchetype.AddSynonym("Sport shoes");

            itemArchetype.Events.Count().Should().Be(2);
            itemArchetype.Events.ToList()[0].Should().BeOfType<ItemArchetypeCreated>();
            itemArchetype.Events.ToList()[1].As<ItemArchetypeSynonymAdded>().Synonym.Should().Be("Sport shoes");
            itemArchetype.Version.Should().Be(1);
            itemArchetype.Synonyms.Single().Should().Be("Sport shoes");
        }

        [Test]
        public void WhenAddingDuplicateSynonyms_ShouldOnlyContainItOnce() {
            var itemArchetype = new ItemArchetype(Guid.NewGuid(), "Sneakers");

            itemArchetype.AddSynonym("Sport shoes");
            itemArchetype.AddSynonym("Sport shoes");

            itemArchetype.Events.Count().Should().Be(3);
            itemArchetype.Events.ToList()[0].Should().BeOfType<ItemArchetypeCreated>();
            itemArchetype.Events.ToList()[1].As<ItemArchetypeSynonymAdded>().Synonym.Should().Be("Sport shoes");
            itemArchetype.Version.Should().Be(2);
            itemArchetype.Synonyms.Single().Should().Be("Sport shoes");
        }
    }
}