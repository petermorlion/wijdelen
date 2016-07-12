using System.Linq;
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
        private readonly IUserImportService _userImportService;
        private readonly IMailService _mailService;

        public AdminController(IAuthorizer authorizer, ICsvReader csvReader, IUserImportService userImportService, IMailService mailService) {
            _authorizer = authorizer;
            _csvReader = csvReader;
            _userImportService = userImportService;
            _mailService = mailService;

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

            var users = _csvReader.ReadUsers(usersFile.InputStream);
            var userImportResults = _userImportService.ImportUsers(users);
            foreach (var userImportResult in userImportResults.Where(x => x.WasImported)) {
                _mailService.SendUserVerificationMail(userImportResult.UserName);
            }
            
            return View("ImportComplete", userImportResults);
        }
    }
}