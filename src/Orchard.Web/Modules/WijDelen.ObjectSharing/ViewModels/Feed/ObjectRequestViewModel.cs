using System;

namespace WijDelen.ObjectSharing.ViewModels.Feed {
    public class ObjectRequestViewModel : IFeedItemViewModel {
        public string UserName { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
        public int ChatCount { get; set; }
        public string ExtraInfo { get; set; }
    }
}