using System;

namespace WijDelen.ObjectSharing.Models {
    public class ChatMessageRecord {
        public virtual int Id { get; set; }
        public virtual Guid ChatId { get; set; }
        public virtual DateTime DateTime { get; set; }
        public virtual int UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Message { get; set; }
    }
}