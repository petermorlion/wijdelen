using System;

namespace WijDelen.ObjectSharing.Models {
    public class ObjectRequestNotificationRecord {
        public virtual int Id { get; set; }
        public virtual int RequestingUserId { get; set; }
        public virtual int ReceivingUserId { get; set; }
        public virtual Guid ObjectRequestId { get; set; }
        public virtual DateTime SentDateTime { get; set; }
    }
}