using System.Collections.Generic;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.ViewModels {
    public class GroupUsersIndexViewModel {
        public IList<GroupUserEntry> Users { get; set; }
        public dynamic Pager { get; set; }
        public IList<GroupViewModel> Groups { get; set; }
        public int SelectedGroupId { get; set; }
        public IList<string> GroupMembershipStatusses { get; set; }
        public string SelectedGroupMembershipStatus { get; set; }
        public string UserNameSearch { get; set; }
    }
}