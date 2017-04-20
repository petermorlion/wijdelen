using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace WijDelen.UserImport {
    public class AdminMenu : INavigationProvider {
        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(item => item
                .Caption(T("Peergroups"))
                .Position("0")
                .Add(subItem => subItem
                    .Caption(T("Groups"))
                    .Position("2")
                    .Action("List", "Admin", new { area = "Contents", id="Group" })
                    .Permission(StandardPermissions.SiteOwner))
                .Add(subItem => subItem
                    .Caption(T("User Import"))
                    .Position("3")
                    .Action("Index", "Admin", new { area = "WijDelen.UserImport" })
                    .Permission(StandardPermissions.SiteOwner)));
        }
    }
}