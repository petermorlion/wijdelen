using Orchard.Users.Models;

namespace WijDelen.UserImport.ViewModels {
    public class GroupUserEntry {
        public UserPartRecord User { get; set; }
        public bool IsChecked { get; set; }
    }
}