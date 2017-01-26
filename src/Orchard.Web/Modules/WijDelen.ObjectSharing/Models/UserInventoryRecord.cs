using System;
using WijDelen.ObjectSharing.Domain.Enums;

namespace WijDelen.ObjectSharing.Models {
    public class UserInventoryRecord {
        public virtual int Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual int SynonymId { get; set; }
        public virtual ObjectRequestAnswer Answer { get; set; }
        public virtual DateTime DateTimeAnswered { get; set; }
    }
}