using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.EventHandlers.Notifications;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Services;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;
using IMailService = WijDelen.ObjectSharing.Infrastructure.IMailService;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class UserNotifierTests {
        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            _groupServiceMock = new Mock<IGroupService>();
            _mailServiceMock = new Mock<IMailService>();
            _getUserByIdQueryMock = new Mock<IGetUserByIdQuery>();
            _findOtherUsersQueryMock = new Mock<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();
            _notificationService1 = new Mock<IUserNotificationService>();
            _notificationService2 = new Mock<IUserNotificationService>();

            var services = new FakeOrchardServices();
            _notifierMock = new Mock<INotifier>();
            services.Notifier = _notifierMock.Object;

            builder.RegisterInstance(_groupServiceMock.Object).As<IGroupService>();
            builder.RegisterInstance(_mailServiceMock.Object).As<IMailService>();
            builder.RegisterInstance(_getUserByIdQueryMock.Object).As<IGetUserByIdQuery>();
            builder.RegisterInstance(_findOtherUsersQueryMock.Object).As<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();
            builder.RegisterInstance(_notificationService1.Object).As<IUserNotificationService>();
            builder.RegisterInstance(_notificationService2.Object).As<IUserNotificationService>();
            builder.RegisterInstance(services).As<IOrchardServices>();
            builder.RegisterType<RandomSampleService>().As<IRandomSampleService>();

            builder.RegisterType<UserNotifier>();

            var fakeUserFactory = new UserFactory();
            _requestingUser = fakeUserFactory.Create("jos", "jos@example.com", "Jos", "Joskens");
            _otherUser = fakeUserFactory.Create("peter.morlion", "peter.morlion@gmail.com", "Peter", "Morlion");
            _unsubscribedUser = fakeUserFactory.Create("john.doe@example.com", "john.doe@example.com", "John", "Doe", false);
            _pendingUser = fakeUserFactory.Create("jane.doe@example.com", "jane.doe@example.com", "Jane", "Doe", true, GroupMembershipStatus.Pending);

            _getUserByIdQueryMock.Setup(x => x.GetResult(_requestingUser.Id)).Returns(_requestingUser);

            _groupServiceMock.Setup(x => x.GetGroupForUser(_requestingUser.Id)).Returns(new GroupViewModel {Name = "Group"});

            var container = builder.Build();
            _userNotifier = container.Resolve<UserNotifier>();

            _userNotifier.T = NullLocalizer.Instance;
        }

        private UserNotifier _userNotifier;
        private Mock<IGroupService> _groupServiceMock;
        private Mock<IMailService> _mailServiceMock;
        private Mock<IGetUserByIdQuery> _getUserByIdQueryMock;
        private Mock<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery> _findOtherUsersQueryMock;
        private Mock<INotifier> _notifierMock;
        private IUser _otherUser;
        private IUser _unsubscribedUser;
        private IUser _pendingUser;
        private IUser _requestingUser;
        private Mock<IUserNotificationService> _notificationService1;
        private Mock<IUserNotificationService> _notificationService2;

        [Test]
        public void WhenBlockedObjectRequestIsUnblocked()
        {
            var objectRequestId = Guid.NewGuid();
            var objectRequestUnblocked = new ObjectRequestUnblocked
            {
                UserId = _requestingUser.Id,
                Description = "Sextant",
                ExtraInfo = "For sextanting",
                SourceId = objectRequestId
            };

            _findOtherUsersQueryMock
                .Setup(x => x.GetResults(_requestingUser.Id, "Sextant"))
                .Returns(new[] { _otherUser, _unsubscribedUser, _pendingUser });

            _userNotifier.Handle(objectRequestUnblocked);

            _notificationService1.Verify(x => x.Handle(It.Is((IEnumerable<IUser> users) => users.Contains(_otherUser) && users.Contains(_unsubscribedUser)), objectRequestUnblocked));
            _notificationService2.Verify(x => x.Handle(It.Is((IEnumerable<IUser> users) => users.Contains(_otherUser) && users.Contains(_unsubscribedUser)), objectRequestUnblocked));

            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Thank you for your request. We sent your request to the members of your group.")), Times.Never);
        }

        [Test]
        public void WhenForbiddenObjectIsRequested()
        {
            var objectRequestId = Guid.NewGuid();
            var sendObjectRequestedNotificationRequested = new ObjectRequested
            {
                UserId = 3,
                Description = "Sex",
                ExtraInfo = "for sexing",
                SourceId = objectRequestId,
                Status = ObjectRequestStatus.BlockedForForbiddenWords
            };

            _userNotifier.Handle(sendObjectRequestedNotificationRequested);

            _notificationService1.Verify(x => x.Handle(It.IsAny<IEnumerable<IUser>>(), It.IsAny<ObjectRequested>()), Times.Never);
            _notificationService2.Verify(x => x.Handle(It.IsAny<IEnumerable<IUser>>(), It.IsAny<ObjectRequested>()), Times.Never);

            _notifierMock.Verify(x => x.Add(NotifyType.Warning, new LocalizedString("Thank you for your request. We noticed some words that might be considered offensive. If our system flagged this incorrectly, we will send your request to the members of your group.")));
            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Thank you for your request. We sent your request to the members of your group.")), Times.Never);
        }

        [Test]
        public void WhenObjectBlockedByAdminIsUnblocked() {
            var objectRequestUnblocked = new ObjectRequestUnblocked {
                WasPreviouslyBlockedByAdmin = true
            };

            _userNotifier.Handle(objectRequestUnblocked);

            _mailServiceMock.Verify(x => x.SendObjectRequestMail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<IUser>>()), Times.Never);

            _notifierMock.Verify(x => x.Add(It.IsAny<NotifyType>(), It.IsAny<LocalizedString>()), Times.Never);

            _notificationService1.Verify(x => x.Handle(It.IsAny<IEnumerable<IUser>>(), It.IsAny<ObjectRequested>()), Times.Never);
            _notificationService2.Verify(x => x.Handle(It.IsAny<IEnumerable<IUser>>(), It.IsAny<ObjectRequested>()), Times.Never);
        }

        [Test]
        public void WhenObjectIsRequested() {
            var objectRequestId = Guid.NewGuid();
            var objectRequested = new ObjectRequested {
                UserId = _requestingUser.Id,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                SourceId = objectRequestId
            };

            _findOtherUsersQueryMock
                .Setup(x => x.GetResults(_requestingUser.Id, "Sneakers"))
                .Returns(new[] {_otherUser, _unsubscribedUser, _pendingUser});

            _userNotifier.Handle(objectRequested);

            _notificationService1.Verify(x => x.Handle(It.Is((IEnumerable<IUser> users) => users.Contains(_otherUser) && users.Contains(_unsubscribedUser)), objectRequested));
            _notificationService2.Verify(x => x.Handle(It.Is((IEnumerable<IUser> users) => users.Contains(_otherUser) && users.Contains(_unsubscribedUser)), objectRequested));

            _mailServiceMock.Verify(x => x.SendAdminObjectRequestMail("Jos Joskens", "Sneakers", "For sneaking"));

            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Thank you for your request. We sent your request to the members of your group.")));
        }

        [Test]
        public void WhenObjectRequestBlocked() {
            var forbiddenWords = new List<string> {"sex"};
            var sourceId = Guid.NewGuid();
            var objectRequestBlocked = new ObjectRequestBlocked {
                UserId = _requestingUser.Id,
                Description = "Sextant",
                ExtraInfo = "For sextanting",
                ForbiddenWords = forbiddenWords,
                SourceId = sourceId
            };

            _userNotifier.Handle(objectRequestBlocked);

            _mailServiceMock.Verify(x => x.SendAdminObjectRequestBlockedMail(sourceId, "Jos Joskens", "Sextant", "For sextanting", forbiddenWords));
        }

        [Test]
        public void WhenObjectRequestBlockedByAdmin() {
            var sourceId = Guid.NewGuid();
            var objectRequestBlocked = new ObjectRequestBlockedByAdmin {
                UserId = _requestingUser.Id,
                Description = "Sneakers",
                SourceId = sourceId,
                Reason = "Reason"
            };

            _userNotifier.Handle(objectRequestBlocked);

            _mailServiceMock.Verify(x => x.SendObjectRequestBlockedMail(_requestingUser, sourceId, "Sneakers", "Reason"));
        }
    }
}