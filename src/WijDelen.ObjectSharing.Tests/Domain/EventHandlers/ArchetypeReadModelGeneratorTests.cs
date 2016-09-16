using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ArchetypeReadModelGeneratorTests {
        [Test]
        public void WhenArchetypeCreated_ShouldSaveReadModel() {
            var repositoryMock = new Mock<IRepository<ArchetypeRecord>>();
            ArchetypeRecord newRecord = null;
            repositoryMock.Setup(x => x.Update(It.IsAny<ArchetypeRecord>())).Callback((ArchetypeRecord r) => newRecord = r);
            var generator = new ArchetypeReadModelGenerator(repositoryMock.Object);
            var e = new ArchetypeCreated { Name = "Sneakers" };

            generator.Handle(e);

            newRecord.Should().NotBeNull();
            newRecord.Name.Should().Be("Sneakers");
        }
    }
}