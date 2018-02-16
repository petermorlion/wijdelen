using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Services;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class UserNotifierTests {
        private UserNotifier _userNotifier;
        private IUser _requestingUser;
        private IUser _pendingUser;
        private IUser _otherUser1;
        private IUser _otherUser2;
        private IUser _unsubscribedUser;
        private readonly IList<ObjectRequestedNotification> _notifications = new List<ObjectRequestedNotification>();

        [SetUp]
        public void Init() {
            var fakeUserFactory = new UserFactory();
            _requestingUser = fakeUserFactory.Create("jos", "jos@example.com", "Jos", "Joskens");
            _otherUser1 = fakeUserFactory.Create("peter.morlion", "peter.morlion@gmail.com", "Peter", "Morlion");
            _otherUser2 = fakeUserFactory.Create("moe", "moe@simpsons.com", "Moe", "Szyslak");
            _unsubscribedUser = fakeUserFactory.Create("john.doe@example.com", "john.doe@example.com", "John", "Doe", false);
            _pendingUser = fakeUserFactory.Create("jane.doe@example.com", "jane.doe@example.com", "Jane", "Doe", true, GroupMembershipStatus.Pending);

            var findOtherUsersQueryMock = new Mock<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();
            findOtherUsersQueryMock
                .Setup(x => x.GetResults(_requestingUser.Id, "sneakers"))
                .Returns(new[] {_otherUser1, _otherUser2, _unsubscribedUser, _pendingUser});

            var notificationRepositoryMock = new Mock<IEventSourcedRepository<ObjectRequestedNotification>>();
            notificationRepositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequestedNotification>(), It.IsAny<string>()))
                .Callback((ObjectRequestedNotification n, string cId) => { _notifications.Add(n); });

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(findOtherUsersQueryMock.Object).As<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();
            containerBuilder.RegisterType<RandomSampleService>().As<IRandomSampleService>();
            containerBuilder.RegisterInstance(notificationRepositoryMock.Object).As<IEventSourcedRepository<ObjectRequestedNotification>>();
            containerBuilder.RegisterType<UserNotifier>();

            var container = containerBuilder.Build();

            _userNotifier = container.Resolve<UserNotifier>();
        }

        [Test]
        public void ObjectRequested_ShouldNotifyUsers() {
            var objectRequestId = Guid.NewGuid();

            var sendRequested = new ObjectRequested {
                Description = "sneakers",
                ExtraInfo = "for sneaking",
                UserId = _requestingUser.Id,
                SourceId = objectRequestId
            };

            _userNotifier.Handle(sendRequested);

            _notifications.Count.Should().Be(3);

            var notification1 = _notifications.Single(x => x.ReceivingUserId == _otherUser1.Id);
            notification1.RequestingUserId.Should().Be(_requestingUser.Id);
            notification1.Description.Should().Be("sneakers");
            notification1.ExtraInfo.Should().Be("for sneaking");
            notification1.ObjectRequestId.Should().Be(objectRequestId);

            var notification2 = _notifications.Single(x => x.ReceivingUserId == _otherUser2.Id);
            notification2.RequestingUserId.Should().Be(_requestingUser.Id);
            notification2.Description.Should().Be("sneakers");
            notification2.ExtraInfo.Should().Be("for sneaking");
            notification2.ObjectRequestId.Should().Be(objectRequestId);

            var notification3 = _notifications.Single(x => x.ReceivingUserId == _unsubscribedUser.Id);
            notification3.RequestingUserId.Should().Be(_requestingUser.Id);
            notification3.Description.Should().Be("sneakers");
            notification3.ExtraInfo.Should().Be("for sneaking");
            notification3.ObjectRequestId.Should().Be(objectRequestId);
        }
    }
}