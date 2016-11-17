using System;

namespace WijDelen.ObjectSharing.Models {
    /// <summary>
    /// A message representing an event that will be read by the message processor.
    /// </summary>
    public class MessageRecord {
        public virtual int Id { get; set; }
        public virtual string Body { get; set; }
        public virtual string CorrelationId { get; set; }
        public virtual DateTime? DeliveryDate { get; set; }
    }
}