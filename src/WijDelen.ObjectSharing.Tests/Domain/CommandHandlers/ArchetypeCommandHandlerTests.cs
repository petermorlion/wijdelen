using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.CommandHandlers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Tests.Domain.CommandHandlers {
    [TestFixture]
    public class ArchetypeCommandHandlerTests {
        [Test]
        public void WhenHandlingCreateCommand() {
            Archetype archetype = null;
            var command = new CreateArchetype("Sneakers");
            var repositoryMock = new Mock<IEventSourcedRepository<Archetype>>();
            repositoryMock.Setup(x => x.Save(It.IsAny<Archetype>(), command.Id.ToString())).Callback((Archetype a, string correlationId) => archetype = a);
            var commandHandler = new ArchetypeCommandHandler(repositoryMock.Object);

            commandHandler.Handle(command);

            archetype.Id.Should().Be(command.ArchetypeId);
            archetype.Name.Should().Be("Sneakers");
            archetype.Events.Single().ShouldBeEquivalentTo(new ArchetypeCreated
            {
                Name = "Sneakers"
            }, options => options.Excluding(o => o.SourceId));
        }


        [Test]
        public void WhenHandlingAddSynonymCommand()
        {
            Archetype updatedArchetype = null;
            var id = Guid.NewGuid();
            var synonymArchetypes = new Dictionary<Guid, IList<string>> {
                { id, new List<string> {  "Sporting shoes" } }
            };

            var command = new SetSynonymArchetypes(synonymArchetypes);
            var archetype = new Archetype(id, new IVersionedEvent[] {
                new ArchetypeCreated { Name = "Sneakers", SourceId = id, Version = 0 },
                new ArchetypeSynonymAdded { Synonym = "Baskets", SourceId = id, Version = 1 }
            });

            var repositoryMock = new Mock<IEventSourcedRepository<Archetype>>();
            repositoryMock.Setup(x => x.Find(id)).Returns(archetype);
            repositoryMock.Setup(x => x.Save(It.IsAny<Archetype>(), command.Id.ToString())).Callback((Archetype a, string correlationId) => updatedArchetype = a);
            var commandHandler = new ArchetypeCommandHandler(repositoryMock.Object);

            commandHandler.Handle(command);

            updatedArchetype.Id.Should().Be(id);
            updatedArchetype.Events.Count().Should().Be(2);
            updatedArchetype.Events.First().ShouldBeEquivalentTo(new ArchetypeSynonymAdded
            {
                Synonym = "Sporting shoes",
                Version = 2
            }, options => options.Excluding(o => o.SourceId));
            updatedArchetype.Events.Last().ShouldBeEquivalentTo(new ArchetypeSynonymRemoved
            {
                Synonym = "Baskets",
                Version = 3
            }, options => options.Excluding(o => o.SourceId));
        }
    }
}