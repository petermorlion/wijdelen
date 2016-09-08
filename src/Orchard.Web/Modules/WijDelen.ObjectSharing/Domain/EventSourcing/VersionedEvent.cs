namespace WijDelen.ObjectSharing.Domain.EventSourcing {
    public abstract class VersionedEvent : IVersionedEvent
    {
        public int SourceId { get; set; }

        public int Version { get; set; }
    }
}