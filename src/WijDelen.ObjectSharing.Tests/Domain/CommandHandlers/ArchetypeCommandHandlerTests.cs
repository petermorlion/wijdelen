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
            archetype.Events.Single().ShouldBeEquivalentTo(new ArchetypeCreated()
            {
                Name = "Sneakers",
            }, options => options.Excluding(o => o.SourceId));
        }
    }
}