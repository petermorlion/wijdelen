using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Entities
{
    public class MailCampaign : EventSourced {
        private MailCampaign(Guid id) : base(id) {
            Handles<MailCampaignCreated>(OnMailCampaignCreated);
        }

        public MailCampaign(Guid id, int userId, IEnumerable<string> emailAddresses) : this(id) {
            Update(new MailCampaignCreated { UserId = userId, EmailAddresses = emailAddresses });
        }

        public MailCampaign(Guid id, IEnumerable<VersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        private void OnMailCampaignCreated(MailCampaignCreated mailCampaignCreated) {
            UserId = mailCampaignCreated.UserId;
            EmailAddresses = mailCampaignCreated.EmailAddresses;
        }

        /// <summary>
        /// The id of the user that requested the object to start this mail campaign.
        /// </summary>
        public int UserId { get; private set; }

        /// <summary>
        /// The email addresses to send this email campaign to.
        /// </summary>
        public IEnumerable<string> EmailAddresses { get; set; }
    }
}