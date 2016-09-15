using System.ComponentModel.DataAnnotations;

namespace WijDelen.ObjectSharing.ViewModels {
    public class NewObjectRequestViewModel {
        [Required]
        public string Description { get; set; }

        [Required]
        public string ExtraInfo { get; set; }
    }
}