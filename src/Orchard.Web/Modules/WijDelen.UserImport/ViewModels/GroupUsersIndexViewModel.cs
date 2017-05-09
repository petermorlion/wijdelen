using System.Collections.Generic;

namespace WijDelen.UserImport.ViewModels {
    public class GroupUsersIndexViewModel {
        public IList<GroupUserEntry> Users { get; set; }
        public dynamic Pager { get; set; }
    }
}