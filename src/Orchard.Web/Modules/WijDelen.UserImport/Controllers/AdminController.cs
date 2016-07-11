using System.Web;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Security;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;

namespace WijDelen.UserImport.Controllers {
    public class AdminController : Controller {
        private readonly IAuthorizer _authorizer;
        private readonly ICsvReader _csvReader;

        public AdminController(IAuthorizer authorizer, ICsvReader csvReader) {
            _authorizer = authorizer;
            _csvReader = csvReader;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_authorizer.Authorize(StandardPermissions.SiteOwner, T("You are not authorized to import users."))) {
                return new HttpUnauthorizedResult();
            }

            return View(new AdminIndexViewModel());
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase usersFile) {
            if (!_authorizer.Authorize(StandardPermissions.SiteOwner, T("You are not authorized to import users."))) {
                return new HttpUnauthorizedResult();
            }
            
            return View("ImportComplete", new ImportCompleteViewModel());
        }
    }
}