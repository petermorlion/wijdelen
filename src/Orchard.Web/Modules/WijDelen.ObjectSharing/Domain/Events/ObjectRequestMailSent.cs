using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.ValueTypes;

namespace WijDelen.ObjectSharing.Domain.Events {
    /// <summary>
    /// Indicates that there was a request to send the mail of an object request. Doesn't mean
    /// the mails have been sent yet (because the mail service might be down for example).
    /// </summary>
    public class ObjectRequestMailSent : VersionedEvent {
        /// <summary>
        /// The recipients of the mail.
        /// </summary>
        public IEnumerable<UserEmail> Recipients { get; set; }

        public string EmailHtml { get; set; }

        public int RequestingUserId { get; set; }

        public Guid ObjectRequestId { get; set; }

        public DateTime SentDateTime { get; set; }
    }
}