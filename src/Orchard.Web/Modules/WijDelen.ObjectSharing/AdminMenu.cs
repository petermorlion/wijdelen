using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace WijDelen.ObjectSharing {
    public class AdminMenu : INavigationProvider {
        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName => "admin";

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("peergroups")
                .Add(item => item
                    .Caption(T("Peergroups"))
                    .Position("13")
                    .Action("Index", "Archetypes", new {area = "WijDelen.ObjectSharing"})
                    .LinkToFirstChild(false)
                    .Add(subItem => subItem
                        .Caption(T("Archetypes"))
                        .Position("1")
                        .LinkToFirstChild(true)
                        .Add(tab => tab
                            .Caption(T("Archetypes"))
                            .Position("0")
                            .Action("Index", "Archetypes", new {area = "WijDelen.ObjectSharing"})
                            .Permission(StandardPermissions.SiteOwner)
                            .LocalNav())
                        .Add(tab => tab
                            .Caption(T("Synonyms"))
                            .Position("1")
                            .Action("Synonyms", "Archetypes", new {area = "WijDelen.ObjectSharing"})
                            .Permission(StandardPermissions.SiteOwner)
                            .LocalNav())));
        }
    }
}