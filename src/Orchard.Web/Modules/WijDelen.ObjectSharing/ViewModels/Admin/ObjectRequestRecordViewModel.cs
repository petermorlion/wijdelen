using System;

namespace WijDelen.ObjectSharing.ViewModels.Admin {
    public class ObjectRequestRecordViewModel {
        public Guid AggregateId { get; set; }
        public bool IsSelected { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}