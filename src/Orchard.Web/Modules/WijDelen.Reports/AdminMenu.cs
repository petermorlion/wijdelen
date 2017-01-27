using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace WijDelen.Reports {
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName
        {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(item => item
                .Caption(T("Peergroups"))
                .Add(subItem => subItem
                    .Caption(T("Reports"))
                    .Position("5")
                    .LinkToFirstChild(true)
                    .Permission(StandardPermissions.SiteOwner)
                    .Add(tab => tab
                        .Caption(T("Overview"))
                        .Action("Index", "Admin", new { area = "WijDelen.Reports" })
                        .LocalNav())
                    .Add(tab => tab
                        .Caption(T("Details"))
                        .Action("Details", "Admin", new { area = "WijDelen.Reports" })
                        .LocalNav())));
        }
    }
}