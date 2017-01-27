using System;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ObjectRequestDenied : VersionedEvent {
        public int DenyingUserId { get; set; }
        public DateTime DateTimeDenied { get; set; }
    }
}