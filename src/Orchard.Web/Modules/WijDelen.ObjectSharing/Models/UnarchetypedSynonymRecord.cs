namespace WijDelen.ObjectSharing.Models {
    /// <summary>
    /// Represents a synonym that hasn't been assigned to an archetype yet.
    /// </summary>
    public class UnarchetypedSynonymRecord {
        public virtual int Id { get; set; } 
        public virtual string Synonym { get; set; } 
    }
}