using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class UserInventoryCreated : VersionedEvent {
        public int UserId { get; set; }
    }
}