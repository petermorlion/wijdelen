using System;

namespace WijDelen.ObjectSharing.Models {
    /// <summary>
    /// Represents the read model of a synonym with its archetype (optional).
    /// </summary>
    public class ArchetypedSynonymRecord {
        public virtual int Id { get; set; } 
        public virtual string Synonym { get; set; } 
        public virtual string Archetype { get; set; }
        public virtual Guid ArchetypeId { get; set; }
    }
}