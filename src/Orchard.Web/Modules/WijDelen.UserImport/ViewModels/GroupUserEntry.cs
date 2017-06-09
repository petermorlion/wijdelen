using Orchard.Users.Models;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.ViewModels {
    public class GroupUserEntry {
        public UserPartRecord User { get; set; }
        public bool IsChecked { get; set; }
        public string GroupName {get; set; }
        public GroupMembershipStatus GroupMembershipStatus { get; set; }
    }
}