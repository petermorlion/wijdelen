using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ArchetypeMarkedAsNotOwned : VersionedEvent
    {
        public int UserId { get; set; }
        public int ArchetypeId { get; set; }
        public string ArchetypeTitle { get; set; }
    }
}