using System.Collections.Generic;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Controllers {
    [Admin]
    public class ConfirmResendUserInvitationMailController : Controller {
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;
        private readonly IMailService _mailService;
        private readonly IGroupService _groupService;

        public ConfirmResendUserInvitationMailController(
            IMembershipService membershipService, 
            IOrchardServices orchardServices, 
            IMailService mailService,
            IGroupService groupService) {
            _membershipService = membershipService;
            _orchardServices = orchardServices;
            _mailService = mailService;
            _groupService = groupService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index(string userName, string returnUrl) {
            var user = _membershipService.GetUser(userName);
            var groupViewModel = _groupService.GetGroupForUser(user.Id);
            if (groupViewModel == null) {
                _orchardServices.Notifier.Add(NotifyType.Warning, T("The user needs to be part of a group first."));
                return Redirect(returnUrl);
            }

            var viewModel = new ConfirmResendUserInvitationMailViewModel {
                UserId = user.Id,
                UserName = userName,
                ReturnUrl = returnUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        [ValidateInput(false)]
        public ActionResult IndexPOST(ConfirmResendUserInvitationMailViewModel viewModel) {
            var user = _membershipService.GetUser(viewModel.UserName);
            var culture = user.As<UserDetailsPart>()?.Culture;
            var groupViewModel = _groupService.GetGroupForUser(user.Id);
            SendUserInvitationMails(culture, new[] { user }, groupViewModel.Name, groupViewModel.LogoUrl, viewModel.Text);
            _orchardServices.Notifier.Add(NotifyType.Success, T("User invitation mail has been sent."));
            return Redirect(viewModel.ReturnUrl);
        }

        private void SendUserInvitationMails(string culture, IEnumerable<IUser> users, string groupName, string groupLogoUrl, string extraInfoHtml) {
            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;

            if (string.IsNullOrWhiteSpace(siteUrl)) {
                siteUrl = HttpContext.Request.ToRootUrlString();
            }

            _mailService.SendUserInvitationMails(culture, users, nonce => Url.MakeAbsolute(Url.Action("Index", "Register", new { Area = "WijDelen.UserImport", nonce = nonce }), siteUrl), groupName, groupLogoUrl, extraInfoHtml);
        }
    }
}