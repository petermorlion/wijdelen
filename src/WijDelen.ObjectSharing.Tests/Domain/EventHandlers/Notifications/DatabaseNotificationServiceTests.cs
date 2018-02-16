using System;
using System.Linq.Expressions;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.EventHandlers.Notifications;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers.Notifications {
    [TestFixture]
    public class DatabaseNotificationServiceTests {
        private IUser _requestingUser;
        private IUser _otherUser;
        private DatabaseNotificationService _service;
        private Mock<IRepository<ReceivedObjectRequestRecord>> _receivedObjectRequestRepositoryMock;
        private ReceivedObjectRequestRecord _persistedRecord;

        [SetUp]
        public void Init() {
            var fakeUserFactory = new UserFactory();
            _requestingUser = fakeUserFactory.Create("jos", "jos@example.com", "Jos", "Joskens");
            _otherUser = fakeUserFactory.Create("peter.morlion", "peter.morlion@gmail.com", "Peter", "Morlion");

            _receivedObjectRequestRepositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            _receivedObjectRequestRepositoryMock
                .Setup(x => x.Create(It.IsAny<ReceivedObjectRequestRecord>()))
                .Callback((ReceivedObjectRequestRecord r) => _persistedRecord = r);

            var builder = new ContainerBuilder();
            builder.RegisterType<DatabaseNotificationService>();
            builder.RegisterInstance(_receivedObjectRequestRepositoryMock.Object).As<IRepository<ReceivedObjectRequestRecord>>();
            var container = builder.Build();

            _service = container.Resolve<DatabaseNotificationService>();
        }

        [Test]
        public void ObjectRequested_ShouldStoreRecord() {
            var sourceId = Guid.NewGuid();
            var createdDateTime = DateTime.UtcNow;
            _service.Handle(new []{_otherUser }, new ObjectRequested{SourceId = sourceId, Description = "sneakers", ExtraInfo = "for sneaking", UserId = _requestingUser.Id, CreatedDateTime = createdDateTime});

            _persistedRecord.ShouldBeEquivalentTo(new ReceivedObjectRequestRecord {
                Description = "sneakers",
                ExtraInfo = "for sneaking",
                ObjectRequestId = sourceId,
                ReceivedDateTime = createdDateTime,
                RequestingUserId = _requestingUser.Id,
                UserId = _otherUser.Id
            });
        }

        [Test]
        public void ObjectRequestUnblocked_ShouldOnlySendToSubscribedUsers() {
            var sourceId = Guid.NewGuid();

            _service.Handle(new[] { _otherUser }, new ObjectRequestUnblocked { SourceId = sourceId, Description = "sneakers", ExtraInfo = "for sneaking", UserId = _requestingUser.Id });

            _persistedRecord.ShouldBeEquivalentTo(new ReceivedObjectRequestRecord
            {
                Description = "sneakers",
                ExtraInfo = "for sneaking",
                ObjectRequestId = sourceId,
                ReceivedDateTime = DateTime.UtcNow,
                RequestingUserId = _requestingUser.Id,
                UserId = _otherUser.Id
            }, options => options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 5000))
                .When(info => info.SelectedMemberPath == "ReceivedDateTime"));
        }
    }
}