using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.Fakes;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ArchetypedSynonymGeneratorTests {
        [Test]
        public void WhenHandlingObjectRequested_ShouldSaveNewArchetypedSynonymRecord() {
            var persistentRecords = new[] {
                new ArchetypedSynonymRecord { Synonym = "Sneakers" }
            };

            var repositoryMock = new Mock<IRepository<ArchetypedSynonymRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            ArchetypedSynonymRecord newRecord = null;
            repositoryMock.Setup(x => x.Update(It.IsAny<ArchetypedSynonymRecord>())).Callback((ArchetypedSynonymRecord r) => newRecord = r);

            var aggregateId = Guid.NewGuid();
            var handler = new ArchetypedSynonymGenerator(repositoryMock.Object);
            var e = new ObjectRequested {
                SourceId = aggregateId,
                Version = 0,
                Description = "Sport shoes",
                ExtraInfo = "For sneaking",
                UserId = 22
            };

            handler.Handle(e);

            newRecord.Synonym.Should().Be("Sport shoes");
        }

        [Test]
        public void WhenHandlingObjectRequestedWithExistingArchetypedSynonym_ShouldNotSaveNewArchetypedSynonymRecord()
        {
            var persistentRecords = new[] {
                new ArchetypedSynonymRecord { Synonym = "Sneakers" }
            };

            var repositoryMock = new Mock<IRepository<ArchetypedSynonymRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var aggregateId = Guid.NewGuid();
            var handler = new ArchetypedSynonymGenerator(repositoryMock.Object);
            var e = new ObjectRequested
            {
                SourceId = aggregateId,
                Version = 0,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                UserId = 22
            };

            handler.Handle(e);

            repositoryMock.Verify(x => x.Update(It.IsAny<ArchetypedSynonymRecord>()), Times.Never);
        }
    }
}