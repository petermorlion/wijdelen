using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ItemArchetypeCreated : VersionedEvent {
        public string Name { get; set; }
    }
}