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
    public class UnarchetypedSynonymGeneratorTests {
        [Test]
        public void WhenHandlingObjectRequested_ShouldSaveNewUnarchetypedSynonymRecord() {
            var persistentRecords = new[] {
                new UnarchetypedSynonymRecord { Synonym = "Sneakers" }
            };

            var repositoryMock = new Mock<IRepository<UnarchetypedSynonymRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            UnarchetypedSynonymRecord newRecord = null;
            repositoryMock.Setup(x => x.Update(It.IsAny<UnarchetypedSynonymRecord>())).Callback((UnarchetypedSynonymRecord r) => newRecord = r);

            var aggregateId = Guid.NewGuid();
            var handler = new UnarchetypedSynonymGenerator(repositoryMock.Object);
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
        public void WhenHandlingObjectRequestedWithExistingUnarchetypedSynonym_ShouldNotSaveNewUnarchetypedSynonymRecord()
        {
            var persistentRecords = new[] {
                new UnarchetypedSynonymRecord { Synonym = "Sneakers" }
            };

            var repositoryMock = new Mock<IRepository<UnarchetypedSynonymRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var aggregateId = Guid.NewGuid();
            var handler = new UnarchetypedSynonymGenerator(repositoryMock.Object);
            var e = new ObjectRequested
            {
                SourceId = aggregateId,
                Version = 0,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                UserId = 22
            };

            handler.Handle(e);

            repositoryMock.Verify(x => x.Update(It.IsAny<UnarchetypedSynonymRecord>()), Times.Never);
        }
    }
}