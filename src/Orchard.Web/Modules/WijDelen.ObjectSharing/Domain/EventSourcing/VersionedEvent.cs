using System;

namespace WijDelen.ObjectSharing.Domain.EventSourcing {
    public abstract class VersionedEvent : IVersionedEvent
    {
        public Guid SourceId { get; set; }

        public int Version { get; set; }
    }
}