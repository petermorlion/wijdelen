using Orchard.ContentManagement.Records;

namespace WijDelen.UserImport.Models {
    public class UserDetailsPartRecord : ContentPartRecord {
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Culture { get; set; }
    }
}