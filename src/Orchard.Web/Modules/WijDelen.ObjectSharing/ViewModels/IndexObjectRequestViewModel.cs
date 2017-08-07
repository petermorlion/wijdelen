using System;

namespace WijDelen.ObjectSharing.ViewModels {
    /// <summary>
    /// A viewmodel for the object requests on the index page.
    /// </summary>
    public class IndexObjectRequestViewModel {
        public Guid AggregateId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string BlockReason { get; set; }
    }
}