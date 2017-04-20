using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.Users.Services;
using Orchard.Localization;
using Orchard.Users.Events;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;

namespace WijDelen.UserImport.Controllers {
    /// <summary>
    /// A copy of AccountController.LostPassword because we want to control the logic and the UI when the user clicks on the link in the invitation mail.
    /// </summary>
    [Themed]
    public class RegisterController : Controller {
        private readonly IUserService _userService;
        private readonly IMembershipService _membershipService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IUpdateUserDetailsService _updateUserDetailsService;

        public RegisterController(IUserService userService, IMembershipService membershipService, IUserEventHandler userEventHandler, IUpdateUserDetailsService updateUserDetailsService) {
            _userService = userService;
            _membershipService = membershipService;
            _userEventHandler = userEventHandler;
            _updateUserDetailsService = updateUserDetailsService;
        }

        [AlwaysAccessible]
        public ActionResult Index(string nonce) {
            var user = _userService.ValidateLostPassword(nonce);
            if (user == null) {
                return RedirectToAction("LogOn");
            }

            if (user.ContentItem.As<UserDetailsPart>().FirstName != "" && user.ContentItem.As<UserDetailsPart>().LastName != "") {
                return RedirectToAction("LogOn");
            }

            ViewData["PasswordLength"] = MinPasswordLength;

            return View();
        }

        [HttpPost]
        [AlwaysAccessible]
        [ValidateInput(false)]
        public ActionResult Index(string nonce, string newPassword, string confirmPassword, string firstName, string lastName) {
            IUser user;
            if ((user = _userService.ValidateLostPassword(nonce)) == null) {
                return Redirect("~/");
            }

            ViewData["PasswordLength"] = MinPasswordLength;

            if (newPassword == null || newPassword.Length < MinPasswordLength) {
                ModelState.AddModelError("newPassword", T("You must specify a new password of {0} or more characters.", MinPasswordLength));
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal)) {
                ModelState.AddModelError("_FORM", T("The new password and confirmation password do not match."));
            }

            if (string.IsNullOrWhiteSpace(firstName)) { 
                ModelState.AddModelError("firstName", T("You must specify a first name."));
            }

            if (string.IsNullOrWhiteSpace(lastName)) {
                ModelState.AddModelError("lastName", T("You must specify a last name."));
            }

            if (!ModelState.IsValid) {
                return View();
            }

            _membershipService.SetPassword(user, newPassword);

            _updateUserDetailsService.UpdateUserDetails(user, firstName, lastName);

            _userEventHandler.ChangedPassword(user);

            return RedirectToAction("Index", "GetStarted", new {area = "WijDelen.ObjectSharing"});
        }

        public Localizer T { get; set; }

        int MinPasswordLength { 
            get {
                return _membershipService.GetSettings().MinRequiredPasswordLength;
            }
        }
    }
}