using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ArchetypeSynonymAdded : VersionedEvent {
        public string Synonym { get; set; }
    }
}