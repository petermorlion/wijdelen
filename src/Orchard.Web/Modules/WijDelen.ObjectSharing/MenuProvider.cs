using System.Web;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using WijDelen.UserImport.Services;

namespace WijDelen.ObjectSharing {
    public class MenuProvider : IMenuProvider {
        private readonly IAuthenticationService _authenticationService;
        private readonly IGroupService _groupService;

        public MenuProvider(IAuthenticationService authenticationService, IGroupService groupService) {
            _authenticationService = authenticationService;
            _groupService = groupService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetMenu(IContent menu, NavigationBuilder builder) {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) {
                return;
            }

            var user = _authenticationService.GetAuthenticatedUser();
            if (!_groupService.IsMemberOfGroup(user.Id)) {
                return;
            }

            if (menu.As<TitlePart>().Title == "Main Menu") {
                builder.Add(
                    T("New Request"), 
                    "10", 
                    item => item.Action("New", "ObjectRequest", new {area = "WijDelen.ObjectSharing"}));
                builder.Add(
                    T("My Requests"),
                    "20",
                    item => item.Action("Index", "ObjectRequest", new { area = "WijDelen.ObjectSharing" }));
                builder.Add(
                    T("Received Requests"),
                    "20",
                    item => item.Action("Index", "ReceivedObjectRequest", new { area = "WijDelen.ObjectSharing" }));
            }
        }
    }
}