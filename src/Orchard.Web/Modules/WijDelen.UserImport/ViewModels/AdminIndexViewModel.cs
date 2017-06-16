using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WijDelen.UserImport.ViewModels {
    public class AdminIndexViewModel {
        [Required]
        public int SelectedGroupId { get; set; }
        public IEnumerable<GroupViewModel> Groups { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }

        [Required]
        public string UserEmails { get; set; }
        [Required]
        public string CultureForMails { get; set; }
    }
}