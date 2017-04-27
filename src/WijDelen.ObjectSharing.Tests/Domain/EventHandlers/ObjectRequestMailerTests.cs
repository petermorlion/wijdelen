using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
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
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;
using IMailService = WijDelen.ObjectSharing.Infrastructure.IMailService;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ObjectRequestMailerTests {
        [Test]
        public void WhenObjectIsRequested() {
            var objectRequestId = Guid.NewGuid();
            var objectRequested = new ObjectRequested {
                UserId = 3,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                SourceId = objectRequestId
            };

            ObjectRequestMail persistedMail = null;

            var fakeUserFactory = new UserFactory();
            var otherUser = fakeUserFactory.Create("peter.morlion", "peter.morlion@gmail.com", "Peter", "Morlion");
            var requestingUser = fakeUserFactory.Create("jos", "jos@example.com", "Jos", "Joskens");

            var getUserByIdQueryMock = new Mock<IGetUserByIdQuery>();
            getUserByIdQueryMock.Setup(x => x.GetResult(3)).Returns(requestingUser);

            var findOtherUsersQueryMock = new Mock<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();
            findOtherUsersQueryMock
                .Setup(x => x.GetResults(3, "Sneakers"))
                .Returns(new[] { otherUser });

            var groupServiceMock = new Mock<IGroupService>();
            groupServiceMock.Setup(x => x.GetGroupForUser(3)).Returns(new GroupViewModel { Name = "Group" });

            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequestMail>>();
            repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequestMail>(), It.IsAny<string>()))
                .Callback((ObjectRequestMail mail, string correlationId) => { persistedMail = mail; });

            var mailServiceMock = new Mock<IMailService>();

            var services = new FakeOrchardServices();
            var notifierMock = new Mock<INotifier>();
            services.Notifier = notifierMock.Object;

            var handler = new ObjectRequestMailer(repositoryMock.Object, groupServiceMock.Object, mailServiceMock.Object, getUserByIdQueryMock.Object, new RandomSampleService(), findOtherUsersQueryMock.Object, services);

            handler.Handle(objectRequested);

            persistedMail.Should().NotBeNull();
            persistedMail.UserId.Should().Be(3);
            persistedMail.Description.Should().Be("Sneakers");
            persistedMail.ExtraInfo.Should().Be("For sneaking");
            persistedMail.ObjectRequestId.Should().Be(objectRequestId);

            mailServiceMock.Verify(x => x.SendObjectRequestMail(
                "Jos Joskens", 
                "Group",
                objectRequestId,
                "Sneakers", 
                "For sneaking", 
                persistedMail, 
                new UserEmail {UserId = otherUser.Id, Email = "peter.morlion@gmail.com"}));

            repositoryMock.Verify(x => x.Save(persistedMail, It.IsAny<string>()));

            notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Thank you for your request. We sent your request to the members of your group.")));
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

            ObjectRequestMail persistedMail = null;

            var getUserByIdQueryMock = new Mock<IGetUserByIdQuery>();
            var findOtherUsersQueryMock = new Mock<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();
            var groupServiceMock = new Mock<IGroupService>();

            var services = new FakeOrchardServices();
            var notifierMock = new Mock<INotifier>();
            services.Notifier = notifierMock.Object;

            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequestMail>>();
            repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequestMail>(), It.IsAny<string>()))
                .Callback((ObjectRequestMail mail, string correlationId) => { persistedMail = mail; });

            var mailServiceMock = new Mock<IMailService>();

            var handler = new ObjectRequestMailer(repositoryMock.Object, groupServiceMock.Object, mailServiceMock.Object, getUserByIdQueryMock.Object, new RandomSampleService(), findOtherUsersQueryMock.Object, services);

            handler.Handle(objectRequested);

            persistedMail.Should().BeNull();

            mailServiceMock.Verify(x => x.SendObjectRequestMail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ObjectRequestMail>(),
                It.IsAny<UserEmail[]>()), Times.Never);

            repositoryMock.Verify(x => x.Save(It.IsAny<ObjectRequestMail>(), It.IsAny<string>()), Times.Never);

            notifierMock.Verify(x => x.Add(NotifyType.Warning, new LocalizedString("Thank you for your request. We noticed some words that might be considered offensive. If our system flagged this incorrectly, we will send your request to the members of your group.")));
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

            ObjectRequestMail persistedMail = null;

            var fakeUserFactory = new UserFactory();
            var otherUser = fakeUserFactory.Create("peter.morlion", "peter.morlion@gmail.com", "Peter", "Morlion");
            var requestingUser = fakeUserFactory.Create("jos", "jos@example.com", "Jos", "Joskens");

            var getUserByIdQueryMock = new Mock<IGetUserByIdQuery>();
            getUserByIdQueryMock.Setup(x => x.GetResult(3)).Returns(requestingUser);

            var findOtherUsersQueryMock = new Mock<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();
            findOtherUsersQueryMock
                .Setup(x => x.GetResults(3, "Sextant"))
                .Returns(new[] { otherUser });

            var groupServiceMock = new Mock<IGroupService>();
            groupServiceMock.Setup(x => x.GetGroupForUser(3)).Returns(new GroupViewModel { Name = "Group" });

            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequestMail>>();
            repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequestMail>(), It.IsAny<string>()))
                .Callback((ObjectRequestMail mail, string correlationId) => { persistedMail = mail; });

            var mailServiceMock = new Mock<IMailService>();

            var handler = new ObjectRequestMailer(repositoryMock.Object, groupServiceMock.Object, mailServiceMock.Object, getUserByIdQueryMock.Object, new RandomSampleService(), findOtherUsersQueryMock.Object, default(IOrchardServices));

            handler.Handle(objectRequestUnblocked);

            persistedMail.Should().NotBeNull();
            persistedMail.UserId.Should().Be(3);
            persistedMail.Description.Should().Be("Sextant");
            persistedMail.ExtraInfo.Should().Be("For sextanting");
            persistedMail.ObjectRequestId.Should().Be(objectRequestId);

            mailServiceMock.Verify(x => x.SendObjectRequestMail(
                "Jos Joskens",
                "Group",
                objectRequestId,
                "Sextant",
                "For sextanting",
                persistedMail,
                new UserEmail { UserId = otherUser.Id, Email = "peter.morlion@gmail.com" }));

            repositoryMock.Verify(x => x.Save(persistedMail, It.IsAny<string>()));
        }
    }
}