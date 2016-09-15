using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Infrastructure;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.Infrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Infrastructure {
    [TestFixture]
    public class OrchardEventSourcedRepositoryTests {
        [Test]
        public void ShouldSaveEventsAndRehydrateWhenGetting() {
            var id = Guid.Parse("16b5f0b6-4498-4cfe-ad3a-6124760f8139");
            var correlationId = "abcd";

            var aggregate = new EventSourcedAggregate(id);
            aggregate.Modify();

            var persistentRecords = new List<EventRecord>();
            
            var repositoryMock = new Mock<IRepository<EventRecord>>();
            repositoryMock
                .Setup(x => x.Update(It.IsAny<EventRecord>()))
                .Callback((EventRecord e) => { persistentRecords.Add(e); });
            repositoryMock
                .Setup(x => x.Fetch(It.IsAny<Expression<Func<EventRecord, bool>>>()))
                .Returns((Expression<Func<EventRecord, bool>> expression) => {
                    var func = expression.Compile();
                    return persistentRecords.Where(func).ToList();
                });

            var orchardEventSourcedRepository = new OrchardEventSourcedRepository<EventSourcedAggregate>(repositoryMock.Object);
            
            orchardEventSourcedRepository.Save(aggregate, correlationId);
            var persistentAggregate = orchardEventSourcedRepository.Find(id);

            persistentAggregate.Should().NotBeNull();
        }
    }
}