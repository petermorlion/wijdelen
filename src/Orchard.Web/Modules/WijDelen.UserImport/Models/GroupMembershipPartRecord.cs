using Orchard.ContentManagement.Records;

namespace WijDelen.UserImport.Models {
    public class GroupMembershipPartRecord : ContentPartRecord {
        public virtual ContentItemRecord Group { get; set; }

        public virtual GroupMembershipStatus GroupMembershipStatus { get; set; }
    }
}