using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WijDelen.UserImport.ViewModels {
    public class AdminIndexViewModel {
        public string NewGroupName { get; set; }
        public int SelectedGroupId { get; set; }
        public IEnumerable<GroupViewModel> Groups { get; set; }

        [Required, FileExtensions(Extensions = "csv")]
        public HttpPostedFileBase File { get; set; }
    }
}