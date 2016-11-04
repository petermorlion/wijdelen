using FluentAssertions;
using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

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


            var handler = new ObjectRequestMailer(repositoryMock.Object);

            handler.Handle(objectRequested);

            persistedMail.Should().NotBeNull();
            persistedMail.UserId.Should().Be(3);
            persistedMail.Description.Should().Be("Sneakers");
            persistedMail.ExtraInfo.Should().Be("For sneaking");
        }
    }
}