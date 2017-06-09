using System.ComponentModel;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace WijDelen.UserImport.Models {
    public class GroupMembershipPart : ContentPart<GroupMembershipPartRecord> {
        public LazyField<IContent> GroupField { get; } = new LazyField<IContent>();

        public IContent Group {
            get { return GroupField.Value; }
            set { GroupField.Value = value; }
        }

        [DisplayName("Group membership status")]
        public GroupMembershipStatus GroupMembershipStatus {
            get { return Retrieve(r => r.GroupMembershipStatus); }
            set { Store(r => r.GroupMembershipStatus, value); }
        }
    }
}