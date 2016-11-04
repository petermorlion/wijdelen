using System;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.ObjectSharing.Domain.Events;

namespace WijDelen.ObjectSharing.Tests.Domain.Entities {
    [TestFixture]
    public class ObjectRequestMailTests {
        [Test]
        public void WhenCreatingObjectRequestMail() {
            var id = Guid.NewGuid();
            var objectRequestMail = new ObjectRequestMail(
                id, 
                22, 
                new[] {"peter.morlion@gmail.com", "peter.morlion@telenet.be"},
                "Sneakers",
                "For sneaking");

            objectRequestMail.Id.Should().Be(id);
            objectRequestMail.UserId.Should().Be(22);
            objectRequestMail.EmailAddresses.Should().BeEquivalentTo("peter.morlion@gmail.com", "peter.morlion@telenet.be");
            objectRequestMail.Description.Should().Be("Sneakers");
            objectRequestMail.ExtraInfo.Should().Be("For sneaking");

            objectRequestMail.Events.Single().ShouldBeEquivalentTo(new ObjectRequestMailCreated {
                SourceId = id,
                UserId = 22,
                EmailAddresses = new[] { "peter.morlion@gmail.com", "peter.morlion@telenet.be" }
            });

            objectRequestMail.Version.Should().Be(0);
            objectRequestMail.Status.Should().Be(ObjectRequestMailStatus.Created);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var previousEvent = new ObjectRequestMailCreated {
                UserId = 22,
                EmailAddresses = new[] { "peter.morlion@gmail.com", "peter.morlion@telenet.be" },
                Description = "Sneakers",
                ExtraInfo = "For sneaking"
            };

            var objectRequestMail = new ObjectRequestMail(id, new [] {previousEvent});

            objectRequestMail.Events.Should().BeEmpty();
            objectRequestMail.Version.Should().Be(0);
            objectRequestMail.Id.Should().Be(id);
            objectRequestMail.UserId.Should().Be(22);
            objectRequestMail.EmailAddresses.Should().BeEquivalentTo("peter.morlion@gmail.com", "peter.morlion@telenet.be");
            objectRequestMail.Description.Should().Be("Sneakers");
            objectRequestMail.ExtraInfo.Should().Be("For sneaking");
            objectRequestMail.Status.Should().Be(ObjectRequestMailStatus.Created);
        }

        [Test]
        public void WhenMarkingAsSent() {
            var id = Guid.NewGuid();
            var objectRequestMail = new ObjectRequestMail(
                id,
                22,
                new[] { "peter.morlion@gmail.com", "peter.morlion@telenet.be" },
                "Sneakers",
                "For sneaking");

            objectRequestMail.MarkAsSent();

            objectRequestMail.Status.Should().Be(ObjectRequestMailStatus.Sent);
            objectRequestMail.Version.Should().Be(1);
            objectRequestMail.Events.Count().Should().Be(2);
            objectRequestMail.Events.Last().ShouldBeEquivalentTo(new ObjectRequestMailSent
            {
                SourceId = id,
                Version = 1
            });

            objectRequestMail.Version.Should().Be(1);
        }
    }
}