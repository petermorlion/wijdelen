using System;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;

namespace WijDelen.ObjectSharing.Tests.Domain.Entities {
    [TestFixture]
    public class MailCampaignTests {
        [Test]
        public void WhenCreatingMailCampaign() {
            var id = Guid.NewGuid();
            var mailCampaign = new MailCampaign(id, 22);

            mailCampaign.Id.Should().Be(id);
            mailCampaign.UserId.Should().Be(22);

            mailCampaign.Events.Single().ShouldBeEquivalentTo(new MailCampaignCreated {
                SourceId = id,
                UserId = 22
            });

            mailCampaign.Version.Should().Be(0);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var previousEvent = new MailCampaignCreated {
                UserId = 22
            };

            var objectRequest = new MailCampaign(id, new [] {previousEvent});

            objectRequest.Events.Should().BeEmpty();
            objectRequest.Version.Should().Be(0);
            objectRequest.Id.Should().Be(id);
            objectRequest.UserId.Should().Be(22);
        }
    }
}