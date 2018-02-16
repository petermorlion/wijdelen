using System;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ObjectRequestedNotificationSent : VersionedEvent {
        public Guid ObjectRequestId { get; set; }
    }
}