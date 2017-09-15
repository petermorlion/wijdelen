using System;

namespace WijDelen.ObjectSharing.Models {
    /// <summary>
    /// A read model of an object request
    /// </summary>
    public class ObjectRequestRecord {
        public virtual int Id { get; set; }
        public virtual Guid AggregateId { get; set; }
        public virtual string Description { get; set; }
        public virtual string ExtraInfo { get; set; }
        public virtual int UserId { get; set; }
        public virtual DateTime CreatedDateTime { get; set; }
        public virtual int GroupId { get; set; }
        public virtual string GroupName { get; set; }
        public virtual string Status { get; set; }
        public virtual string BlockReason { get; set; }
        public virtual int ChatCount { get; set; }
    }
}