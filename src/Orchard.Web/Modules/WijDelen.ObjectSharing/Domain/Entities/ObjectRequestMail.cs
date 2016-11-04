using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Entities
{
    /// <summary>
    /// Aggregate root representing the mail that is sent out when requesting a new object.
    /// </summary>
    public class ObjectRequestMail : EventSourced {
        private ObjectRequestMail(Guid id) : base(id) {
            Handles<ObjectRequestMailCreated>(OnMailCampaignCreated);
        }

        public ObjectRequestMail(Guid id, int userId, IEnumerable<string> emailAddresses) : this(id) {
            Update(new ObjectRequestMailCreated { UserId = userId, EmailAddresses = emailAddresses });
        }

        public ObjectRequestMail(Guid id, IEnumerable<VersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        private void OnMailCampaignCreated(ObjectRequestMailCreated objectRequestMailCreated) {
            UserId = objectRequestMailCreated.UserId;
            EmailAddresses = objectRequestMailCreated.EmailAddresses;
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