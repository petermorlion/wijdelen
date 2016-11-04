using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace WijDelen.ObjectSharing {
    public class MenuProvider : IMenuProvider {
        public MenuProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetMenu(IContent menu, NavigationBuilder builder) {
            if (menu.As<TitlePart>().Title == "Main Menu") {
                builder.Add(T("New Request"), "10", item => item.Action("New", "ObjectRequest", new { area = "WijDelen.ObjectSharing" }));
            }
        }
    }
}