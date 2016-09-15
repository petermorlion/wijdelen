using System;

namespace WijDelen.ObjectSharing.Models {
    public class EventRecord {
        public virtual int Id { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual Guid AggregateId { get; set; }
        public virtual string AggregateType { get; set; }
        public virtual int Version { get; set; }
        public virtual string Payload { get; set; }
        public virtual string CorrelationId { get; set; }

    }
}