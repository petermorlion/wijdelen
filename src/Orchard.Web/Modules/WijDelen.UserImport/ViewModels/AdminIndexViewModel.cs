using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WijDelen.UserImport.ViewModels {
    public class AdminIndexViewModel {
        [Required]
        public int SelectedGroupId { get; set; }
        public IEnumerable<GroupViewModel> Groups { get; set; }

        [Required]
        public string UserEmails { get; set; }
    }
}