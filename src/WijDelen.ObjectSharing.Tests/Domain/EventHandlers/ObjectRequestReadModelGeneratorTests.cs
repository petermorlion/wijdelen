using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ObjectRequestReadModelGeneratorTests {
        [Test]
        public void WhenHandlingObjectRequested_ShouldSaveNewObjectRequestRecord() {
            var persistentRecords = new[] {
                new ObjectRequestRecord { AggregateId = Guid.NewGuid() }
            };

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);
            
            ObjectRequestRecord newRecord = null;
            repositoryMock.Setup(x => x.Create(It.IsAny<ObjectRequestRecord>())).Callback((ObjectRequestRecord r) => newRecord = r);

            var groupServiceMock = new Mock<IGroupService>();
            groupServiceMock.Setup(x => x.GetGroupForUser(22)).Returns(new GroupViewModel {
                Id = 123,
                Name = "The Flying Hellfish"
            });

            var aggregateId = Guid.NewGuid();
            var handler = new ObjectRequestReadModelGenerator(repositoryMock.Object, groupServiceMock.Object);
            var e = new ObjectRequested {
                SourceId = aggregateId,
                Version = 0,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                UserId = 22,
                CreatedDateTime = new DateTime(2016, 12, 27)
            };

            handler.Handle(e);

            newRecord.AggregateId.Should().Be(aggregateId);
            newRecord.Version.Should().Be(0);
            newRecord.Description.Should().Be("Sneakers");
            newRecord.ExtraInfo.Should().Be("For sneaking");
            newRecord.UserId.Should().Be(22);
            newRecord.CreatedDateTime.Should().Be(new DateTime(2016, 12, 27));
            newRecord.GroupId.Should().Be(123);
            newRecord.GroupName.Should().Be("The Flying Hellfish");
        }
    }
}