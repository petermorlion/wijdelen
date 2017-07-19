using System;
using System.Collections.Generic;

namespace WijDelen.ObjectSharing.ViewModels.Admin {
    public class ObjectRequestDetailsAdminViewModel {
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string ExtraInfo { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IList<string> ForbiddenWords { get; set; }
        public string BlockReason { get; set; }
        public bool CanBlock { get; set; }
        public bool CanUnblock { get; set; }
    }
}