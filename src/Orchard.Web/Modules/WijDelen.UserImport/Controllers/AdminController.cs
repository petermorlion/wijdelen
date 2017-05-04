using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Utility.Extensions;
using WijDelen.UserImport.Services;
using Orchard;
using Orchard.UI.Notify;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Controllers {
    public class AdminController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly ICsvReader _csvReader;
        private readonly IUserImportService _userImportService;
        private readonly IMailService _mailService;
        private readonly IGroupService _groupService;
        private readonly IMembershipService _membershipService;
        private readonly INotifier _notifier;

        public AdminController(
            IOrchardServices orchardServices, 
            ICsvReader csvReader, 
            IUserImportService userImportService, 
            IMailService mailService,
            IGroupService groupService,
            IMembershipService membershipService,
            INotifier notifier) {
            _orchardServices = orchardServices;
            _csvReader = csvReader;
            _userImportService = userImportService;
            _mailService = mailService;
            _groupService = groupService;
            _membershipService = membershipService;
            _notifier = notifier;

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
            string groupLogoUrl = "";
            if (viewModel.UserImportLinkMode == UserImportLinkMode.New) {
                groupName = viewModel.NewGroupName;
            }
            else {
                var groupViewModel = _groupService.GetGroups().Single(g => g.Id == viewModel.SelectedGroupId);
                groupName = groupViewModel.Name;
                groupLogoUrl = groupViewModel.LogoUrl;
            }

            _groupService.AddUsersToGroup(groupName, userImportResults.Where(u => u.WasImported && u.User != null).Select(u => u.User));

            SendUserVerificationMails(userImportResults.Where(x => x.WasImported).Select(x => x.User), groupName, groupLogoUrl);
            
            return View("ImportComplete", userImportResults);
        }

        public ActionResult ResendUserVerificationMail(string userName) {
            var user = _membershipService.GetUser(userName);
            var groupViewModel = _groupService.GetGroupForUser(user.Id);
            SendUserVerificationMails(new[] { user }, groupViewModel.Name, groupViewModel.LogoUrl);
            _notifier.Add(NotifyType.Success, T("User verification mail has been sent."));
            return RedirectToAction("Edit", "Admin", new {area = "Orchard.Users", id = user.Id});
        }

        private void SendUserVerificationMails(IEnumerable<IUser> users, string groupName, string groupLogoUrl) {
            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;

            if (string.IsNullOrWhiteSpace(siteUrl)) {
                siteUrl = HttpContext.Request.ToRootUrlString();
            }

            _mailService.SendUserVerificationMails(users, nonce => Url.MakeAbsolute(Url.Action("Index", "Register", new { Area = "WijDelen.UserImport", nonce = nonce }), siteUrl), groupName, groupLogoUrl);
        }
    }
}