using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace WijDelen.UserImport.Models {
    public class NamePart : ContentPart<NamePartRecord> {
        [Required]
        [DisplayName("A name.")]
        public string Name {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }
    }
}