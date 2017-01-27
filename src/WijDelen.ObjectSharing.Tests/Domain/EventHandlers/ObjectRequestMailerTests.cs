using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Services;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
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

            var otherUserMock = new Mock<IUser>();
            otherUserMock.Setup(x => x.Id).Returns(22);
            otherUserMock.Setup(x => x.Email).Returns("peter.morlion@gmail.com");

            var requestingUserMock = new Mock<IUser>();
            requestingUserMock.Setup(x => x.UserName).Returns("Jos");

            var getUserByIdQueryMock = new Mock<IGetUserByIdQuery>();
            getUserByIdQueryMock.Setup(x => x.GetResult(3)).Returns(requestingUserMock.Object);

            var groupServiceMock = new Mock<IGroupService>();
            groupServiceMock
                .Setup(x => x.GetOtherUsersInGroup(3))
                .Returns(new[] { otherUserMock.Object });

            groupServiceMock.Setup(x => x.GetGroupForUser(3)).Returns(new GroupViewModel { Name = "Group" });

            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequestMail>>();
            repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequestMail>(), It.IsAny<string>()))
                .Callback((ObjectRequestMail mail, string correlationId) => { persistedMail = mail; });

            var mailServiceMock = new Mock<IMailService>();

            var handler = new ObjectRequestMailer(repositoryMock.Object, groupServiceMock.Object, mailServiceMock.Object, getUserByIdQueryMock.Object, new RandomSampleService());

            handler.Handle(objectRequested);

            persistedMail.Should().NotBeNull();
            persistedMail.UserId.Should().Be(3);
            persistedMail.Description.Should().Be("Sneakers");
            persistedMail.ExtraInfo.Should().Be("For sneaking");
            persistedMail.ObjectRequestId.Should().Be(objectRequestId);

            mailServiceMock.Verify(x => x.SendObjectRequestMail(
                "Jos", 
                "Group",
                objectRequestId,
                "Sneakers", 
                "For sneaking", 
                persistedMail, 
                new UserEmail {UserId = 22, Email = "peter.morlion@gmail.com"}));

            repositoryMock.Verify(x => x.Save(persistedMail, It.IsAny<string>()));
        }
    }
}