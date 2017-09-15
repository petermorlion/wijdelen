using System;

namespace WijDelen.ObjectSharing.ViewModels.Feed {
    public class ObjectRequestViewModel {
        public string UserName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int ChatCount { get; set; }
    }
}