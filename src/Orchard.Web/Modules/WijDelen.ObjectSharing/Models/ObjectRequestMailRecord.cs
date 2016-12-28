using System;

namespace WijDelen.ObjectSharing.Models {
    public class ObjectRequestMailRecord {
        public virtual int Id { get; set; }
        public virtual Guid AggregateId { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual string EmailHtml { get; set; }
        public virtual int RequestingUserId { get; set; }
        public virtual int ReceivingUserId { get; set; }
        public virtual Guid ObjectRequestId { get; set; }
        public virtual DateTime SentDateTime { get; set; }
    }
}