using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ObjectRequestReadModelGeneratorTests {
        [Test]
        public void WhenHandlingObjectRequested_ShouldSaveNewObjectRequestRecord() {
            var persistentRecords = new[] {
                new ObjectRequestRecord { AggregateId = Guid.NewGuid() }
            };

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock
                .Setup(x => x.Fetch(It.IsAny<Expression<Func<ObjectRequestRecord, bool>>>()))
                .Returns((Expression<Func<ObjectRequestRecord, bool>> expression) => {
                    var func = expression.Compile();
                    return persistentRecords.Where(func).ToList();
                });

            ObjectRequestRecord newRecord = null;
            repositoryMock.Setup(x => x.Update(It.IsAny<ObjectRequestRecord>())).Callback((ObjectRequestRecord r) => newRecord = r);

            var aggregateId = Guid.NewGuid();
            var handler = new ObjectRequestReadModelGenerator(repositoryMock.Object);
            var e = new ObjectRequested {
                SourceId = aggregateId,
                Version = 0,
                Description = "Sneakers",
                ExtraInfo = "For sneaking"
            };

            handler.Handle(e);

            newRecord.AggregateId.Should().Be(aggregateId);
            newRecord.Version.Should().Be(0);
            newRecord.Description.Should().Be("Sneakers");
            newRecord.ExtraInfo.Should().Be("For sneaking");
        }
    }
}