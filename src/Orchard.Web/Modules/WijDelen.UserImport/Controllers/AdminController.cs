using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Utility.Extensions;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using Orchard;

namespace WijDelen.UserImport.Controllers {
    public class AdminController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly ICsvReader _csvReader;
        private readonly IUserImportService _userImportService;
        private readonly IMailService _mailService;

        public AdminController(IOrchardServices orchardServices, ICsvReader csvReader, IUserImportService userImportService, IMailService mailService) {
            _orchardServices = orchardServices;
            _csvReader = csvReader;
            _userImportService = userImportService;
            _mailService = mailService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You are not authorized to import users."))) {
                return new HttpUnauthorizedResult();
            }

            return View(new AdminIndexViewModel());
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase usersFile) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You are not authorized to import users."))) {
                return new HttpUnauthorizedResult();
            }

            var users = _csvReader.ReadUsers(usersFile.InputStream);
            var userImportResults = _userImportService.ImportUsers(users);

            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;

            if (string.IsNullOrWhiteSpace(siteUrl)) {
                siteUrl = HttpContext.Request.ToRootUrlString();
            }

            _mailService.SendUserVerificationMails(userImportResults.Where(x => x.WasImported), nonce => Url.MakeAbsolute(Url.Action("SetPassword", "Account", new { Area = "WijDelen.UserImport", nonce = nonce }), siteUrl));
            
            return View("ImportComplete", userImportResults);
        }
    }
}