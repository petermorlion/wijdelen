using System.Linq;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Utility.Extensions;
using WijDelen.UserImport.Services;
using Orchard;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Controllers {
    public class AdminController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly ICsvReader _csvReader;
        private readonly IUserImportService _userImportService;
        private readonly IMailService _mailService;
        private readonly IGroupService _groupService;

        public AdminController(
            IOrchardServices orchardServices, 
            ICsvReader csvReader, 
            IUserImportService userImportService, 
            IMailService mailService,
            IGroupService groupService) {
            _orchardServices = orchardServices;
            _csvReader = csvReader;
            _userImportService = userImportService;
            _mailService = mailService;
            _groupService = groupService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You are not authorized to import users."))) {
                return new HttpUnauthorizedResult();
            }

            var groups = _groupService.GetGroups();
            return View(new AdminIndexViewModel { Groups = groups });
        }

        [HttpPost]
        public ActionResult Index(AdminIndexViewModel viewModel) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You are not authorized to import users."))) {
                return new HttpUnauthorizedResult();
            }

            if (viewModel.UserImportLinkMode == UserImportLinkMode.New && string.IsNullOrEmpty(viewModel.NewGroupName)) {
                ModelState.AddModelError("NewGroupName", T("Please provide a group name to create a new group."));
                return View();
            }

            var users = _csvReader.ReadUsers(viewModel.File.InputStream);
            var userImportResults = _userImportService.ImportUsers(users);

            string groupName;
            if (viewModel.UserImportLinkMode == UserImportLinkMode.New) {
                groupName = viewModel.NewGroupName;
            }
            else {
                groupName = _groupService.GetGroups().Single(g => g.Id == viewModel.SelectedGroupId).Name;
            }

            _groupService.AddUsersToGroup(groupName, userImportResults.Where(u => u.WasImported && u.User != null).Select(u => u.User));

            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;

            if (string.IsNullOrWhiteSpace(siteUrl)) {
                siteUrl = HttpContext.Request.ToRootUrlString();
            }

            _mailService.SendUserVerificationMails(userImportResults.Where(x => x.WasImported), nonce => Url.MakeAbsolute(Url.Action("Index", "Register", new { Area = "WijDelen.UserImport", nonce = nonce }), siteUrl));
            
            return View("ImportComplete", userImportResults);
        }
    }
}