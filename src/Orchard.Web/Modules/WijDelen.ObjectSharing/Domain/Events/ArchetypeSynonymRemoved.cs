using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ArchetypeSynonymRemoved : VersionedEvent {
        public string Synonym { get; set; }
    }
}