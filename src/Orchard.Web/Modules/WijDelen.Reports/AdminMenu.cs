using Orchard.Localization;
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
                .Position("0")
                .Add(subItem => subItem
                    .Caption(T("Reports"))
                    .Position("6")
                    .LinkToFirstChild(true)
                    .Add(tab => tab
                        .Caption(T("Overview"))
                        .Action("Index", "Admin", new { area = "WijDelen.Reports" })
                        .Permission(Permissions.ViewReports)
                        .LocalNav())
                    .Add(tab => tab
                        .Caption(T("Groups"))
                        .Action("Groups", "Admin", new { area = "WijDelen.Reports" })
                        .Permission(Permissions.ViewReports)
                        .LocalNav())
                    .Add(tab => tab
                        .Caption(T("Requests"))
                        .Action("Requests", "Admin", new { area = "WijDelen.Reports" })
                        .Permission(Permissions.ViewReports)
                        .LocalNav())));
        }
    }
}