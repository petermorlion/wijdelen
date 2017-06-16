using System.Collections.Generic;

namespace WijDelen.ObjectSharing.ViewModels.Admin {
    public class ObjectRequestAdminViewModel {
        public List<ObjectRequestRecordViewModel> ObjectRequests { get; set; }
        public int Page { get; set; }
        public int ObjectRequestsCount { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int TotalPages { get; set; }
    }
}