using System;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ChatStarted : VersionedEvent
    {
        public int RequestingUserId { get; set; }

        public Guid ObjectRequestId { get; set; }

        public int ConfirmingUserId { get; set; }
    }
}