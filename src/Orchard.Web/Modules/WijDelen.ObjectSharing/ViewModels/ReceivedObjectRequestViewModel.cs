using System;

namespace WijDelen.ObjectSharing.ViewModels {
    public class ReceivedObjectRequestViewModel {
        public Guid ObjectRequestId { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string ExtraInfo { get; set; }
        public DateTime ReceivedDateTime { get; set; }
    }
}