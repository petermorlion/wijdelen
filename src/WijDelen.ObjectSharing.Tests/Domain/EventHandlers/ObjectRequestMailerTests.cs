using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.UserImport.Services;
using IMailService = WijDelen.ObjectSharing.Infrastructure.IMailService;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ObjectRequestMailerTests {
        [Test]
        public void WhenObjectIsRequested() {
            var objectRequested = new ObjectRequested {
                UserId = 3,
                Description = "Sneakers",
                ExtraInfo = "For sneaking"
            };

            ObjectRequestMail persistedMail = null;

            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequestMail>>();
            repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequestMail>(), It.IsAny<string>()))
                .Callback((ObjectRequestMail mail, string correlationId) => { persistedMail = mail; });


            var handler = new ObjectRequestMailer(repositoryMock.Object, default(IGroupService), default(IMailService));

            handler.Handle(objectRequested);

            persistedMail.Should().NotBeNull();
            persistedMail.UserId.Should().Be(3);
            persistedMail.Description.Should().Be("Sneakers");
            persistedMail.ExtraInfo.Should().Be("For sneaking");
        }

        [Test]
        public void WhenObjectRequestMailIsCreated() {
            var entityId = Guid.NewGuid();

            var objectRequestMailCreated = new ObjectRequestMailCreated
            {
                UserId = 22,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                SourceId = entityId
            };

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Email).Returns("peter.morlion@gmail.com");

            var entity = new ObjectRequestMail(entityId, 22, "Sneakers", "For sneaking");

            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequestMail>>();
            repositoryMock
                .Setup(x => x.Find(entityId))
                .Returns(entity);
            
            var groupServiceMock = new Mock<IGroupService>();
            groupServiceMock
                .Setup(x => x.GetOtherUsersInGroup(22))
                .Returns(new[] { userMock.Object});

            var mailServiceMock = new Mock<IMailService>();

            var handler = new ObjectRequestMailer(repositoryMock.Object, groupServiceMock.Object, mailServiceMock.Object);

            handler.Handle(objectRequestMailCreated);

            mailServiceMock.Verify(x => x.SendObjectRequestMail("Sneakers", "For sneaking", "peter.morlion@gmail.com"));
            entity.Status.Should().Be(ObjectRequestMailStatus.Sent);
            repositoryMock.Verify(x => x.Save(entity, It.IsAny<string>()));
        }
    }
}