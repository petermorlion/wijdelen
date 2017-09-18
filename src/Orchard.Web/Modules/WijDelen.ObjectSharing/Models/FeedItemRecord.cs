using System;

namespace WijDelen.ObjectSharing.Models {
    public class FeedItemRecord {
        public virtual int Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual Guid ObjectRequestId { get; set; }
        public virtual string SendingUserName { get; set; }
        public virtual Guid? ChatId { get; set; }
        public virtual DateTime DateTime { get; set; }
        public virtual FeedItemType ItemType { get; set; }
        public virtual string Description { get; set; }
        public virtual string ExtraInfo { get; set; }
        public virtual int ConfirmationCount { get; set; }
    }
}