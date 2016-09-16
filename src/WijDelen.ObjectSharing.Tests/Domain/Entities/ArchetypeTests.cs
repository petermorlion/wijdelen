using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Tests.Domain.Entities {
    [TestFixture]
    public class ArchetypeTests {
        [Test]
        public void WhenCreatingArchetype() {
            var id = Guid.NewGuid();
            var itemArchetype = new Archetype(id, "Sneakers");

            itemArchetype.Name.Should().Be("Sneakers");

            itemArchetype.Events.Single().ShouldBeEquivalentTo(new ArchetypeCreated
            {
                SourceId = id,
                Name = "Sneakers"
            });

            itemArchetype.Version.Should().Be(0);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var created = new ArchetypeCreated
            {
                Name = "Sneakers",
                Version = 0
            };

            var synonymAdded = new ArchetypeSynonymAdded
            {
                Synonym = "Sport shoes",
                Version = 1
            };

            var itemArchetype = new Archetype(id, new IVersionedEvent[] { created, synonymAdded });

            itemArchetype.Events.Should().BeEmpty();
            itemArchetype.Version.Should().Be(1);
            itemArchetype.Name.Should().Be("Sneakers");
            itemArchetype.Synonyms.Single().Should().Be("Sport shoes");
        }

        [Test]
        public void WhenAddingSynonym() {
            var itemArchetype = new Archetype(Guid.NewGuid(), "Sneakers");

            itemArchetype.AddSynonym("Sport shoes");

            itemArchetype.Events.Count().Should().Be(2);
            itemArchetype.Events.ToList()[0].Should().BeOfType<ArchetypeCreated>();
            itemArchetype.Events.ToList()[1].As<ArchetypeSynonymAdded>().Synonym.Should().Be("Sport shoes");
            itemArchetype.Version.Should().Be(1);
            itemArchetype.Synonyms.Single().Should().Be("Sport shoes");
        }

        [Test]
        public void WhenAddingDuplicateSynonyms_ShouldOnlyContainItOnce() {
            var itemArchetype = new Archetype(Guid.NewGuid(), "Sneakers");

            itemArchetype.AddSynonym("Sport shoes");
            itemArchetype.AddSynonym("Sport shoes");

            itemArchetype.Events.Count().Should().Be(2);
            itemArchetype.Events.ToList()[0].Should().BeOfType<ArchetypeCreated>();
            itemArchetype.Events.ToList()[1].As<ArchetypeSynonymAdded>().Synonym.Should().Be("Sport shoes");
            itemArchetype.Version.Should().Be(1);
            itemArchetype.Synonyms.Single().Should().Be("Sport shoes");
        }

        [Test]
        public void WhenRemovingSynonym()
        {
            var itemArchetype = new Archetype(Guid.NewGuid(), "Sneakers");

            itemArchetype.AddSynonym("Sport shoes");
            itemArchetype.RemoveSynonym("Sport shoes");

            itemArchetype.Events.Count().Should().Be(3);
            itemArchetype.Events.ToList()[0].Should().BeOfType<ArchetypeCreated>();
            itemArchetype.Events.ToList()[1].Should().BeOfType<ArchetypeSynonymAdded>();
            itemArchetype.Events.ToList()[2].As<ArchetypeSynonymRemoved>().Synonym.Should().Be("Sport shoes");
            itemArchetype.Version.Should().Be(2);
            itemArchetype.Synonyms.Single().Should().BeEmpty();
        }

        [Test]
        public void WhenRemovingSynonymTwice()
        {
            var itemArchetype = new Archetype(Guid.NewGuid(), "Sneakers");

            itemArchetype.AddSynonym("Sport shoes");
            itemArchetype.RemoveSynonym("Sport shoes");
            itemArchetype.RemoveSynonym("Sport shoes");

            itemArchetype.Events.Count().Should().Be(3);
            itemArchetype.Events.ToList()[0].Should().BeOfType<ArchetypeCreated>();
            itemArchetype.Events.ToList()[1].Should().BeOfType<ArchetypeSynonymAdded>();
            itemArchetype.Events.ToList()[2].As<ArchetypeSynonymRemoved>().Synonym.Should().Be("Sport shoes");
            itemArchetype.Version.Should().Be(2);
            itemArchetype.Synonyms.Single().Should().BeEmpty();
        }
    }
}