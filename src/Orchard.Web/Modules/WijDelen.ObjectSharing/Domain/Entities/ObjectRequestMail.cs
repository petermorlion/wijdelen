using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Enums;
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
            Handles<ObjectRequestMailSent>(OnObjectRequestMailSent);
        }

        public ObjectRequestMail(Guid id, int userId, string description, string extraInfo) : this(id) {
            Update(new ObjectRequestMailCreated { UserId = userId, Description = description, ExtraInfo = extraInfo });
        }

        public ObjectRequestMail(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        public void MarkAsSent(IEnumerable<string> recipients, string emailHtml) {
            Update(new ObjectRequestMailSent {
                Recipients = recipients,
                EmailHtml = emailHtml,
                RequestingUserId = UserId
            });
        }

        private void OnMailCampaignCreated(ObjectRequestMailCreated objectRequestMailCreated) {
            UserId = objectRequestMailCreated.UserId;
            Description = objectRequestMailCreated.Description;
            ExtraInfo = objectRequestMailCreated.ExtraInfo;
        }

        private void OnObjectRequestMailSent(ObjectRequestMailSent objectRequestMailSent) {
            Recipients = objectRequestMailSent.Recipients;
            Status = ObjectRequestMailStatus.Sent;
        }

        /// <summary>
        /// The id of the user that requested the object to start this mail campaign.
        /// </summary>
        public int UserId { get; private set; }

        /// <summary>
        /// The recipients of this mail.
        /// </summary>
        public IEnumerable<string> Recipients { get; private set; }

        /// <summary>
        /// A short description of the object
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Extra info as to why the user needs it, what he/she plans to do with it, etc.
        /// </summary>
        public string ExtraInfo { get; private set; }

        public ObjectRequestMailStatus Status { get; private set; }
    }
}