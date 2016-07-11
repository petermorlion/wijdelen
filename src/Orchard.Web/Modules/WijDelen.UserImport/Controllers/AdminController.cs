using System.Web.Mvc;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Controllers {
    public class AdminController : Controller {
        private readonly IOrchardServices _orchardServices;

        public AdminController(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You are not authorized to import users."))) {
                return new HttpUnauthorizedResult();
            }

            return View(new AdminIndexViewModel());
        }
    }
}