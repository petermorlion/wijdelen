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
                .Add(menu =>
                {
                    menu.LinkToFirstChild(true)
                        .Caption(T("Archetypes"))
                        .Position("13");

                    menu.Add(item => item
                        .Caption(T("Archetypes"))
                        .Position("0")
                        .Action("Index", "Archetypes", new { area = "WijDelen.ObjectSharing" })
                        .Permission(StandardPermissions.SiteOwner)
                        .LocalNav());

                    menu.Add(item => item
                        .Caption(T("Synonyms without archetype"))
                        .Position("1")
                        .Action("Unarchetyped", "Archetypes", new { area = "WijDelen.ObjectSharing" })
                        .Permission(StandardPermissions.SiteOwner)
                        .LocalNav());
                });
        }
    }
}