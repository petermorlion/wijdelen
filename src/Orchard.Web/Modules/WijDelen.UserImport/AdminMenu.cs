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
                    .Position("2")
                    .Action("List", "Admin", new { area = "Contents", id="Group" })
                    .Permission(Permissions.ManageGroups))
                .Add(subItem => subItem
                    .Caption(T("User Import"))
                    .Position("3")
                    .Action("Index", "Admin", new { area = "WijDelen.UserImport" })
                    .Permission(Permissions.ImportUsers)));

            builder.Add(T("Users"),
                menu => menu.Add(T("Groups"), "3.0", item => item.Action("Index", "GroupUsers", new { area = "WijDelen.UserImport" })
                    .LocalNav()
                    .Permission(Permissions.ManageGroups)));
        }
    }
}