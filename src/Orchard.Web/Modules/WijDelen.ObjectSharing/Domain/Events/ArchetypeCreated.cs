using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ArchetypeCreated : VersionedEvent {
        public string Name { get; set; }
    }

    public class ArchetypeSynonymAdded : VersionedEvent {
        public string Synonym { get; set; }
    }
}