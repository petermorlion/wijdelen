using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ItemArchetypeReadModelGeneratorTests {
        [Test]
        public void WhenItemArchetypeCreated_ShouldSaveReadModel() {
            var repositoryMock = new Mock<IRepository<ItemArchetypeRecord>>();
            ItemArchetypeRecord newRecord = null;
            repositoryMock.Setup(x => x.Update(It.IsAny<ItemArchetypeRecord>())).Callback((ItemArchetypeRecord r) => newRecord = r);
            var generator = new ItemArchetypeReadModelGenerator(repositoryMock.Object);
            var e = new ItemArchetypeCreated { Name = "Sneakers" };

            generator.Handle(e);

            newRecord.Should().NotBeNull();
            newRecord.Name.Should().Be("Sneakers");
        }
    }
}