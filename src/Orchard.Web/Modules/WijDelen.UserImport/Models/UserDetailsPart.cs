using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace WijDelen.UserImport.Models {
    public class UserDetailsPart : ContentPart<UserDetailsPartRecord> {
        [Required]
        [DisplayName("First Name")]
        public string FirstName {
            get { return Retrieve(r => r.FirstName); }
            set { Store(r => r.FirstName, value); }
        }

        [Required]
        [DisplayName("Last Name")]
        public string LastName
        {
            get { return Retrieve(r => r.LastName); }
            set { Store(r => r.LastName, value); }
        }

        public bool IsComplete() {
            return LastName != "" && FirstName != "";
        }
    }
}