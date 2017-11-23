using Orchard.Localization;
using Orchard.UI.Navigation;

namespace WijDelen.UserImport {
    public class AdminMenu : INavigationProvider {
        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName => "admin";

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(item => item
                .Caption(T("Peergroups"))
                .Position("0")
                .Add(subItem => subItem
                    .Caption(T("Groups"))
                    .Position("4")
                    .Action("List", "Admin", new { area = "Contents", id="Group" })
                    .Permission(Permissions.ManageGroups))
                .Add(subItem => subItem
                    .Caption(T("User Import"))
                    .Position("5")
                    .Action("Index", "Admin", new { area = "WijDelen.UserImport" })
                    .Permission(Permissions.ImportUsers))
                .Add(subItem => subItem
                    .Caption(T("Users"))
                    .Position("6")
                    .Action("Index", "GroupUsers", new { area = "WijDelen.UserImport" })
                    .Permission(Permissions.ManageGroups)));
        }
    }
}