using System;

namespace WijDelen.ObjectSharing.Models {
    /// <summary>
    /// The read model for item archetypes.
    /// </summary>
    public class ArchetypeRecord {
        public virtual int Id { get; set; }
        public virtual Guid AggregateId { get; set; }
        public virtual string Name { get; set; }
    }
}