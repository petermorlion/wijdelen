using System;
using Autofac;
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
        private ObjectRequestReadModelGenerator _handler;
        private ObjectRequestRecord _newRecord;
        private ObjectRequestRecord _updatedRecord;
        private Guid _normalAggregateId;
        private Guid _blockedAggregateId;

        [SetUp]
        public void Init() {
            _normalAggregateId = Guid.NewGuid();
            _blockedAggregateId = Guid.NewGuid();
            var persistentRecords = new[] {
                new ObjectRequestRecord {
                    AggregateId = _normalAggregateId
                },
                new ObjectRequestRecord {
                    AggregateId = _blockedAggregateId,
                    Status = "BlockedForForbiddenWords",
                    BlockReason = "Reason"
                }
            };

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            repositoryMock.Setup(x => x.Update(It.IsAny<ObjectRequestRecord>())).Callback((ObjectRequestRecord r) => _updatedRecord = r);
            repositoryMock.Setup(x => x.Create(It.IsAny<ObjectRequestRecord>())).Callback((ObjectRequestRecord r) => _newRecord = r);

            var groupServiceMock = new Mock<IGroupService>();
            groupServiceMock.Setup(x => x.GetGroupForUser(22)).Returns(new GroupViewModel {
                Id = 123,
                Name = "The Flying Hellfish"
            });

            var builder = new ContainerBuilder();
            builder.RegisterInstance(groupServiceMock.Object).As<IGroupService>();
            builder.RegisterInstance(repositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            builder.RegisterType<ObjectRequestReadModelGenerator>();

            var container = builder.Build();
            _handler = container.Resolve<ObjectRequestReadModelGenerator>();
        }

        [Test]
        public void WhenHandlingObjectRequested_ShouldSaveNewObjectRequestRecord() {
            var aggregateId = Guid.NewGuid();
            var e = new ObjectRequested {
                SourceId = aggregateId,
                Version = 0,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                UserId = 22,
                CreatedDateTime = new DateTime(2016, 12, 27),
                Status = ObjectRequestStatus.None
            };

            _handler.Handle(e);

            _newRecord.AggregateId.Should().Be(aggregateId);
            _newRecord.Description.Should().Be("Sneakers");
            _newRecord.ExtraInfo.Should().Be("For sneaking");
            _newRecord.UserId.Should().Be(22);
            _newRecord.CreatedDateTime.Should().Be(new DateTime(2016, 12, 27));
            _newRecord.GroupId.Should().Be(123);
            _newRecord.GroupName.Should().Be("The Flying Hellfish");
            _newRecord.Status.Should().Be("None");
        }

        [Test]
        public void WhenObjectRequestIsUnblocked_ShouldUpdateObjectRequestRecord()
        {
            var e = new ObjectRequestUnblocked
            {
                SourceId = _blockedAggregateId,
                Version = 4,
                Description = "Sextant",
                ExtraInfo = "For sextanting",
                UserId = 22,
                Status = ObjectRequestStatus.None
            };

            _handler.Handle(e);

            _updatedRecord.AggregateId.Should().Be(_blockedAggregateId);
            _updatedRecord.Status.Should().Be("None");
            _updatedRecord.BlockReason.Should().Be("");
        }

        [Test]
        public void WhenObjectRequestIsBlocked_ShouldUpdateObjectRequestRecord()
        {
            var e = new ObjectRequestBlockedByAdmin
            {
                SourceId = _normalAggregateId,
                Version = 4,
                Reason = "Just because"
            };

            _handler.Handle(e);

            _updatedRecord.AggregateId.Should().Be(_normalAggregateId);
            _updatedRecord.Status.Should().Be("BlockedByAdmin");
            _updatedRecord.BlockReason.Should().Be("Just because");
        }

        [Test]
        public void WhenObjectRequestIsStopped() {
            var e = new ObjectRequestStopped {
                SourceId = _normalAggregateId
            };

            _handler.Handle(e);

            _updatedRecord.AggregateId.Should().Be(_normalAggregateId);
            _updatedRecord.Status.Should().Be("Stopped");
        }
    }
}