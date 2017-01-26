using System;
using WijDelen.ObjectSharing.Domain.Enums;

namespace WijDelen.ObjectSharing.Models {
    public class ObjectRequestResponseRecord {
        public virtual int Id { get; set; }
        public virtual Guid ObjectRequestId { get; set; }
        public virtual int UserId { get; set; }
        public virtual ObjectRequestAnswer Response { get; set; }
    }
}