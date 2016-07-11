using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Security;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Controllers {
    public class AdminController : Controller {
        private readonly IAuthorizer _authorizer;

        public AdminController(IAuthorizer authorizer) {
            _authorizer = authorizer;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_authorizer.Authorize(StandardPermissions.SiteOwner, T("You are not authorized to import users."))) {
                return new HttpUnauthorizedResult();
            }

            return View(new AdminIndexViewModel());
        }
    }
}