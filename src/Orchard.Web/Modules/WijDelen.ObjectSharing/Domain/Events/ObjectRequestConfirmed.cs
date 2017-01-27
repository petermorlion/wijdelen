using System;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ObjectRequestConfirmed : VersionedEvent {
        public int ConfirmingUserId { get; set; }
        public DateTime DateTimeConfirmed { get; set; }
    }
}