using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.ValueTypes;
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
                new ObjectRequestRecord {AggregateId = Guid.NewGuid()}
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
                CreatedDateTime = new DateTime(2016, 12, 27),
                Status = ObjectRequestStatus.None
            };

            handler.Handle(e);

            newRecord.AggregateId.Should().Be(aggregateId);
            newRecord.Description.Should().Be("Sneakers");
            newRecord.ExtraInfo.Should().Be("For sneaking");
            newRecord.UserId.Should().Be(22);
            newRecord.CreatedDateTime.Should().Be(new DateTime(2016, 12, 27));
            newRecord.GroupId.Should().Be(123);
            newRecord.GroupName.Should().Be("The Flying Hellfish");
            newRecord.Status.Should().Be("None");
        }

        [Test]
        public void WhenObjectRequestIsUnblocked_ShouldUpdateObjectRequestRecord()
        {
            var aggregateId = Guid.NewGuid();
            var persistentRecords = new[] {
                new ObjectRequestRecord {
                    AggregateId = aggregateId,
                    Status = "BlockedForForbiddenWords",
                    BlockReason = "Reason"
                }
            };

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            ObjectRequestRecord updatedRecord = null;
            repositoryMock.Setup(x => x.Update(It.IsAny<ObjectRequestRecord>())).Callback((ObjectRequestRecord r) => updatedRecord = r);

            var groupServiceMock = new Mock<IGroupService>();
            groupServiceMock.Setup(x => x.GetGroupForUser(22)).Returns(new GroupViewModel
            {
                Id = 123,
                Name = "The Flying Hellfish"
            });

            var handler = new ObjectRequestReadModelGenerator(repositoryMock.Object, groupServiceMock.Object);
            var e = new ObjectRequestUnblocked
            {
                SourceId = aggregateId,
                Version = 4,
                Description = "Sextant",
                ExtraInfo = "For sextanting",
                UserId = 22,
                Status = ObjectRequestStatus.None
            };

            handler.Handle(e);

            updatedRecord.AggregateId.Should().Be(aggregateId);
            updatedRecord.Status.Should().Be("None");
            updatedRecord.BlockReason.Should().Be("");
        }

        [Test]
        public void WhenObjectRequestIsBlocked_ShouldUpdateObjectRequestRecord()
        {
            var aggregateId = Guid.NewGuid();
            var persistentRecords = new[] {
                new ObjectRequestRecord {
                    AggregateId = aggregateId
                }
            };

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            ObjectRequestRecord updatedRecord = null;
            repositoryMock.Setup(x => x.Update(It.IsAny<ObjectRequestRecord>())).Callback((ObjectRequestRecord r) => updatedRecord = r);

            var groupServiceMock = new Mock<IGroupService>();

            var handler = new ObjectRequestReadModelGenerator(repositoryMock.Object, groupServiceMock.Object);
            var e = new ObjectRequestBlockedByAdmin
            {
                SourceId = aggregateId,
                Version = 4,
                Reason = "Just because"
            };

            handler.Handle(e);

            updatedRecord.AggregateId.Should().Be(aggregateId);
            updatedRecord.Status.Should().Be("BlockedByAdmin");
            updatedRecord.BlockReason.Should().Be("Just because");
        }
    }
}