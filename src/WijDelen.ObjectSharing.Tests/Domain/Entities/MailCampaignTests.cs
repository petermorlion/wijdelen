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
            var mailCampaign = new MailCampaign(id, 22, new[] {"peter.morlion@gmail.com", "peter.morlion@telenet.be"});

            mailCampaign.Id.Should().Be(id);
            mailCampaign.UserId.Should().Be(22);
            mailCampaign.EmailAddresses.Should().BeEquivalentTo("peter.morlion@gmail.com", "peter.morlion@telenet.be");

            mailCampaign.Events.Single().ShouldBeEquivalentTo(new MailCampaignCreated {
                SourceId = id,
                UserId = 22,
                EmailAddresses = new[] { "peter.morlion@gmail.com", "peter.morlion@telenet.be" }
            });

            mailCampaign.Version.Should().Be(0);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var previousEvent = new MailCampaignCreated {
                UserId = 22,
                EmailAddresses = new[] { "peter.morlion@gmail.com", "peter.morlion@telenet.be" }
            };

            var mailCampaign = new MailCampaign(id, new [] {previousEvent});

            mailCampaign.Events.Should().BeEmpty();
            mailCampaign.Version.Should().Be(0);
            mailCampaign.Id.Should().Be(id);
            mailCampaign.UserId.Should().Be(22);
            mailCampaign.EmailAddresses.Should().BeEquivalentTo("peter.morlion@gmail.com", "peter.morlion@telenet.be");
        }
    }
}