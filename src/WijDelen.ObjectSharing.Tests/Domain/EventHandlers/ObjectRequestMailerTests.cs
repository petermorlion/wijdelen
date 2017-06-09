using System;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
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
    public class ObjectRequestMailerTests {
        private ObjectRequestMailer _objectRequestMailer;
        private Mock<IEventSourcedRepository<ObjectRequestMail>> _repositoryMock;
        private Mock<IGroupService> _groupServiceMock;
        private Mock<IMailService> _mailServiceMock;
        private Mock<IGetUserByIdQuery> _getUserByIdQueryMock;
        private Mock<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery> _findOtherUsersQueryMock;
        private Mock<INotifier> _notifierMock;
        private IUser _otherUser;
        private ObjectRequestMail _persistedMail;
        private IUser _unsubscribedUser;
        private IUser _pendingUser;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            _groupServiceMock = new Mock<IGroupService>();
            _repositoryMock = new Mock<IEventSourcedRepository<ObjectRequestMail>>();
            _mailServiceMock = new Mock<IMailService>();
            _getUserByIdQueryMock = new Mock<IGetUserByIdQuery>();
            _findOtherUsersQueryMock = new Mock<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();

            var services = new FakeOrchardServices();
            _notifierMock = new Mock<INotifier>();
            services.Notifier = _notifierMock.Object;

            builder.RegisterInstance(_groupServiceMock.Object).As<IGroupService>();
            builder.RegisterInstance(_repositoryMock.Object).As<IEventSourcedRepository<ObjectRequestMail>>();
            builder.RegisterInstance(_mailServiceMock.Object).As<IMailService>();
            builder.RegisterInstance(_getUserByIdQueryMock.Object).As<IGetUserByIdQuery>();
            builder.RegisterInstance(_findOtherUsersQueryMock.Object).As<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();
            builder.RegisterInstance(services).As<IOrchardServices>();
            builder.RegisterInstance(new RandomSampleService()).As<IRandomSampleService>();

            builder.RegisterType<ObjectRequestMailer>();

            var fakeUserFactory = new UserFactory();
            var requestingUser = fakeUserFactory.Create("jos", "jos@example.com", "Jos", "Joskens");
            _otherUser = fakeUserFactory.Create("peter.morlion", "peter.morlion@gmail.com", "Peter", "Morlion");
            _unsubscribedUser = fakeUserFactory.Create("john.doe@example.com", "john.doe@example.com", "John", "Doe", false);
            _pendingUser = fakeUserFactory.Create("jane.doe@example.com", "jane.doe@example.com", "Jane", "Doe", true, GroupMembershipStatus.Pending);

            _getUserByIdQueryMock.Setup(x => x.GetResult(3)).Returns(requestingUser);

            _groupServiceMock.Setup(x => x.GetGroupForUser(3)).Returns(new GroupViewModel { Name = "Group" });

            _persistedMail = null;
            _repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequestMail>(), It.IsAny<string>()))
                .Callback((ObjectRequestMail mail, string correlationId) => { _persistedMail = mail; });

            var container = builder.Build();
            _objectRequestMailer = container.Resolve<ObjectRequestMailer>();

            _objectRequestMailer.T = NullLocalizer.Instance;
        }

        [Test]
        public void WhenObjectIsRequested() {
            var objectRequestId = Guid.NewGuid();
            var objectRequested = new ObjectRequested {
                UserId = 3,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                SourceId = objectRequestId
            };

            _findOtherUsersQueryMock
                .Setup(x => x.GetResults(3, "Sneakers"))
                .Returns(new[] { _otherUser, _unsubscribedUser, _pendingUser });

            _objectRequestMailer.Handle(objectRequested);

            _persistedMail.Should().NotBeNull();
            _persistedMail.UserId.Should().Be(3);
            _persistedMail.Description.Should().Be("Sneakers");
            _persistedMail.ExtraInfo.Should().Be("For sneaking");
            _persistedMail.ObjectRequestId.Should().Be(objectRequestId);

            _mailServiceMock.Verify(x => x.SendObjectRequestMail(
                "Jos Joskens", 
                "Group",
                objectRequestId,
                "Sneakers", 
                "For sneaking", 
                _persistedMail, 
                It.Is((IUser u) => u.Id == _otherUser.Id && u.Email == "peter.morlion@gmail.com")));

            _repositoryMock.Verify(x => x.Save(_persistedMail, It.IsAny<string>()));

            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Thank you for your request. We sent your request to the members of your group.")));
        }

        [Test]
        public void WhenForbiddenObjectIsRequested()
        {
            var objectRequestId = Guid.NewGuid();
            var objectRequested = new ObjectRequested
            {
                UserId = 3,
                Description = "Sex",
                ExtraInfo = "for sexing",
                SourceId = objectRequestId,
                Status = ObjectRequestStatus.BlockedForForbiddenWords
            };

            _objectRequestMailer.Handle(objectRequested);

            _persistedMail.Should().BeNull();

            _mailServiceMock.Verify(x => x.SendObjectRequestMail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ObjectRequestMail>(),
                It.IsAny<IUser[]>()), Times.Never);

            _repositoryMock.Verify(x => x.Save(It.IsAny<ObjectRequestMail>(), It.IsAny<string>()), Times.Never);

            _notifierMock.Verify(x => x.Add(NotifyType.Warning, new LocalizedString("Thank you for your request. We noticed some words that might be considered offensive. If our system flagged this incorrectly, we will send your request to the members of your group.")));
            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Thank you for your request. We sent your request to the members of your group.")), Times.Never);
        }

        [Test]
        public void WhenBlockedObjectIsUnblocked()
        {
            var objectRequestId = Guid.NewGuid();
            var objectRequestUnblocked = new ObjectRequestUnblocked
            {
                UserId = 3,
                Description = "Sextant",
                ExtraInfo = "For sextanting",
                SourceId = objectRequestId
            };

            _findOtherUsersQueryMock
                .Setup(x => x.GetResults(3, "Sextant"))
                .Returns(new[] { _otherUser });

            _objectRequestMailer.Handle(objectRequestUnblocked);

            _persistedMail.Should().NotBeNull();
            _persistedMail.UserId.Should().Be(3);
            _persistedMail.Description.Should().Be("Sextant");
            _persistedMail.ExtraInfo.Should().Be("For sextanting");
            _persistedMail.ObjectRequestId.Should().Be(objectRequestId);

            _mailServiceMock.Verify(x => x.SendObjectRequestMail(
                "Jos Joskens",
                "Group",
                objectRequestId,
                "Sextant",
                "For sextanting",
                _persistedMail,
                It.Is((IUser u) => u.Id == _otherUser.Id && u.Email == "peter.morlion@gmail.com")));

            _repositoryMock.Verify(x => x.Save(_persistedMail, It.IsAny<string>()));
            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Thank you for your request. We sent your request to the members of your group.")), Times.Never);
        }
    }
}