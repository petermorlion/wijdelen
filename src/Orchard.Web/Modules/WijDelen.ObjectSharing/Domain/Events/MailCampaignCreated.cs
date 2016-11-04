using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class MailCampaignCreated : VersionedEvent
    {
        /// <summary>
        /// The id of the user that requested the object to start this mail campaign.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The email addresses to send this email campaign to.
        /// </summary>
        public IEnumerable<string> EmailAddresses { get; set; }
    }
}