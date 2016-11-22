using System;

namespace WijDelen.ObjectSharing.Models {
    public class ChatRecord {
        public virtual int Id { get; set; }
        public virtual Guid ChatId { get; set; }
        public virtual Guid ObjectRequestId { get; set; }
        public virtual int RequestingUserId { get; set; }
        public virtual string RequestingUserName { get; set; }
        public virtual int ConfirmingUserId { get; set; }
        public virtual string ConfirmingUserName { get; set; }
    }
}