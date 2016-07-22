using System.Web.Mvc;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Services;

namespace WijDelen.UserImport.Controllers {
    public class AccountController  : Controller {
        private readonly IUserService _userService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IMembershipService _membershipService;

        public AccountController(IUserService userService, IUserEventHandler userEventHandler, IMembershipService membershipService) {
            _userService = userService;
            _userEventHandler = userEventHandler;
            _membershipService = membershipService;
        }

        public ActionResult SetPassword(string nonce) {
            var user = _userService.ValidateChallenge(nonce);

            if (user == null) {
                // TODO
            }

            return View();
        }

        [HttpPost]
        public ActionResult SetPassword(string nonce, string password)
        {
            var user = _userService.ValidateChallenge(nonce);

            if (user == null) {
                return RedirectToAction("ChallengeEmailFail", "Account", new { area = "Orchard.Users" });
            }

            if (string.IsNullOrEmpty(password)) {
                ModelState.AddModelError("password", "bleh");
            }

            if (!ModelState.IsValid) {
                return View(); // TODO: show error summary
            }

            _userEventHandler.ConfirmedEmail(user);

            _membershipService.SetPassword(user, password);

            _userEventHandler.ChangedPassword(user);

            return RedirectToAction("ChangePasswordSuccess", "Account", new { area = "Orchard.Users" });
        }
    }
}