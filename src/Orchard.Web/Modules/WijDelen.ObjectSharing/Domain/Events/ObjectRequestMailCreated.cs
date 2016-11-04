using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ObjectRequestMailCreated : VersionedEvent
    {
        /// <summary>
        /// The id of the user that requested the object to start this mail campaign.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The email addresses to send this email campaign to.
        /// </summary>
        public IEnumerable<string> EmailAddresses { get; set; }

        /// <summary>
        /// A short description of the object
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Extra info as to why the user needs it, what he/she plans to do with it, etc.
        /// </summary>
        public string ExtraInfo { get; set; }
    }
}