using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Utility.Extensions;
using WijDelen.UserImport.Services;
using Orchard;
using Orchard.UI.Notify;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Controllers {
    public class AdminController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IUserImportService _userImportService;
        private readonly IMailService _mailService;
        private readonly IGroupService _groupService;
        private readonly IMembershipService _membershipService;
        private readonly INotifier _notifier;

        public AdminController(
            IOrchardServices orchardServices, 
            IUserImportService userImportService, 
            IMailService mailService,
            IGroupService groupService,
            IMembershipService membershipService,
            INotifier notifier) {
            _orchardServices = orchardServices;
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

            var users = viewModel.UserEmails.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var userImportResults = _userImportService.ImportUsers(users);

            var groupViewModel = _groupService.GetGroups().Single(g => g.Id == viewModel.SelectedGroupId);
            var groupName = groupViewModel.Name;
            var groupLogoUrl = groupViewModel.LogoUrl;

            var importedUsers = userImportResults.Where(u => u.WasImported && u.User != null).Select(u => u.User);
            _groupService.AddUsersToGroup(groupName, importedUsers);

            SendUserInvitationMails(importedUsers, groupName, groupLogoUrl);
            
            return View("ImportComplete", userImportResults);
        }

        public ActionResult ResendUserInvitationMail(string userName) {
            var user = _membershipService.GetUser(userName);
            var groupViewModel = _groupService.GetGroupForUser(user.Id);
            if (groupViewModel == null) {
                _notifier.Add(NotifyType.Warning, T("The user needs to be part of a group first."));
            } else {
                SendUserInvitationMails(new[] { user }, groupViewModel.Name, groupViewModel.LogoUrl);
                _notifier.Add(NotifyType.Success, T("User invitation mail has been sent."));
            }

            return RedirectToAction("Edit", "Admin", new {area = "Orchard.Users", id = user.Id});
        }

        private void SendUserInvitationMails(IEnumerable<IUser> users, string groupName, string groupLogoUrl) {
            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;

            if (string.IsNullOrWhiteSpace(siteUrl)) {
                siteUrl = HttpContext.Request.ToRootUrlString();
            }

            _mailService.SendUserInvitationMails(users, nonce => Url.MakeAbsolute(Url.Action("Index", "Register", new { Area = "WijDelen.UserImport", nonce = nonce }), siteUrl), groupName, groupLogoUrl);
        }
    }
}