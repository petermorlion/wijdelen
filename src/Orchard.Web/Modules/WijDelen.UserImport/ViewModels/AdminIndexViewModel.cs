using System.Collections.Generic;
using Orchard.Roles.ViewModels;

namespace WijDelen.UserImport.ViewModels {
    public class AdminIndexViewModel {
        public bool? Approve { get; set; }
        public string UpdateExisting { get; set; }
        public List<UserRoleEntry> Roles { get; set; }
    }
}