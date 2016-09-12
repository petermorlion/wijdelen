using System;

namespace WijDelen.ObjectSharing.Models {
    public class VersionedEventRecord {
        public DateTime Timestamp { get; set; }
        public int AggregateId { get; set; }
        public string AggregateType { get; set; }
        public int Version { get; set; }
        public string Payload { get; set; }
        public string CorrelationId { get; set; }

    }
}