using System;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class SendObjectRequestedNotificationRequested : VersionedEvent {
        /// <summary>
        /// The id of the user that requested the object.
        /// </summary>
        public int RequestingUserId { get; set; }

        /// <summary>
        /// The id of the user that will received the notification.
        /// </summary>
        public int ReceivingUserId { get; set; }

        /// <summary>
        /// A short description of the object
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Extra info as to why the user needs it, what he/she plans to do with it, etc.
        /// </summary>
        public string ExtraInfo { get; set; }

        public Guid ObjectRequestId { get; set; }
    }
}