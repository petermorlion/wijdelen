using System;

namespace WijDelen.ObjectSharing.Models {
    public class ReceivedObjectRequestRecord {
        public virtual int Id { get; set; }
        public virtual Guid ObjectRequestId { get; set; }
        public virtual int UserId { get; set; }
        public virtual string Description { get; set; }
        public virtual string ExtraInfo { get; set; }
        public virtual DateTime ReceivedDateTime { get; set; }
        public virtual int RequestingUserId { get; set; }
    }
}