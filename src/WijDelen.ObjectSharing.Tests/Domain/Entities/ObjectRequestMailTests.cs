using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.ValueTypes;

namespace WijDelen.ObjectSharing.Tests.Domain.Entities {
    [TestFixture]
    public class ObjectRequestMailTests {
        [Test]
        public void WhenCreatingObjectRequestMail() {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var objectRequestMail = new ObjectRequestMail(
                id, 
                22, 
                "Sneakers",
                "For sneaking",
                objectRequestId);

            objectRequestMail.Id.Should().Be(id);
            objectRequestMail.UserId.Should().Be(22);
            objectRequestMail.Description.Should().Be("Sneakers");
            objectRequestMail.ExtraInfo.Should().Be("For sneaking");

            objectRequestMail.Events.Single().ShouldBeEquivalentTo(new ObjectRequestMailCreated {
                SourceId = id,
                UserId = 22,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                ObjectRequestId = objectRequestId
            });

            objectRequestMail.Version.Should().Be(0);
            objectRequestMail.Status.Should().Be(ObjectRequestMailStatus.Created);
            objectRequestMail.ObjectRequestId.Should().Be(objectRequestId);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var previousEvent = new ObjectRequestMailCreated {
                UserId = 22,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                ObjectRequestId = objectRequestId
            };

            var objectRequestMail = new ObjectRequestMail(id, new [] {previousEvent});

            objectRequestMail.Events.Should().BeEmpty();
            objectRequestMail.Version.Should().Be(0);
            objectRequestMail.Id.Should().Be(id);
            objectRequestMail.UserId.Should().Be(22);
            objectRequestMail.Description.Should().Be("Sneakers");
            objectRequestMail.ExtraInfo.Should().Be("For sneaking");
            objectRequestMail.Status.Should().Be(ObjectRequestMailStatus.Created);
            objectRequestMail.ObjectRequestId.Should().Be(objectRequestId);
        }

        [Test]
        public void WhenMarkingAsSent() {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var objectRequestMail = new ObjectRequestMail(
                id,
                22,
                "Sneakers",
                "For sneaking",
                objectRequestId);

            var userEmail1 = new UserEmail {UserId = 1, Email = "peter.morlion@gmail.com"};
            var userEmail2 = new UserEmail {UserId = 2, Email = "peter.morlion@telenet.be" };

            objectRequestMail.MarkAsSent(new [] { userEmail1, userEmail2 }, "HTML");

            objectRequestMail.Status.Should().Be(ObjectRequestMailStatus.Sent);
            objectRequestMail.Recipients.Should().BeEquivalentTo(userEmail1, userEmail2);
            objectRequestMail.Version.Should().Be(1);
            objectRequestMail.Events.Count().Should().Be(2);
            objectRequestMail.Events.Last().ShouldBeEquivalentTo(new ObjectRequestMailSent
            {
                SourceId = id,
                Version = 1,
                Recipients = new List<UserEmail> { userEmail1, userEmail2 },
                EmailHtml = "HTML",
                RequestingUserId = 22,
                ObjectRequestId = objectRequestId
            });

            objectRequestMail.Version.Should().Be(1);
        }
    }
}