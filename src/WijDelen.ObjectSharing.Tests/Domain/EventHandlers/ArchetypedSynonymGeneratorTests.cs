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

        [Test]
        public void WhenHandlingArchetypeSynonymAdded() {
            var archetypedSynonymRecords = new[] {
                new ArchetypedSynonymRecord {
                    Synonym = "Sporting shoes"
                }
            };

            var archetypeSynonymRepositoryMock = new Mock<IRepository<ArchetypedSynonymRecord>>();
            archetypeSynonymRepositoryMock.SetRecords(archetypedSynonymRecords);

            var archetypeRecords = new[] {
                new ArchetypeRecord {
                    Name = "Sneakers",
                    AggregateId = Guid.NewGuid()
                }
            };

            var archetypeRepositoryMock = new Mock<IRepository<ArchetypeRecord>>();
            archetypeRepositoryMock.SetRecords(archetypeRecords);

            var handler = new ArchetypedSynonymGenerator(archetypeSynonymRepositoryMock.Object);

            var e = new ArchetypeSynonymAdded {
                SourceId = archetypeRecords[0].AggregateId,
                Synonym = "Sporting shoes",
                Archetype = "Sneakers"
            };

            ArchetypedSynonymRecord record = null;
            archetypeSynonymRepositoryMock.Setup(x => x.Update(It.IsAny<ArchetypedSynonymRecord>())).Callback((ArchetypedSynonymRecord r) => record = r);

            handler.Handle(e);

            record.Archetype.Should().Be("Sneakers");
            record.ArchetypeId.Should().Be(archetypeRecords[0].AggregateId);
            record.Synonym.Should().Be("Sporting shoes");
        }
    }
}