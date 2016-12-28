using System;

namespace WijDelen.ObjectSharing.Domain.EventSourcing {
    public abstract class VersionedEvent : IVersionedEvent
    {
        public Guid SourceId { get; set; }

        /// <summary>
        /// This property indicates the version of the aggregate that this event applies to.
        /// </summary>
        public int Version { get; set; }
    }
}