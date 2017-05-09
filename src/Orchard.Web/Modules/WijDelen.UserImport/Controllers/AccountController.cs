using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Themes;
using Orchard.UI.Notify;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Controllers {
    [Themed]
    [Authorize]
    public class AccountController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IUpdateUserDetailsService _updateUserDetailsService;

        public AccountController(IOrchardServices orchardServices, IUpdateUserDetailsService updateUserDetailsService) {
            _orchardServices = orchardServices;
            _updateUserDetailsService = updateUserDetailsService;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var user = _orchardServices.WorkContext.CurrentUser;
            var userDetailsPart = user.ContentItem.As<UserDetailsPart>();
            var viewModel = new UserDetailsViewModel {
                FirstName = userDetailsPart.FirstName,
                LastName = userDetailsPart.LastName,
                Culture = userDetailsPart.Culture,
                ReceiveMails = userDetailsPart.ReceiveMails
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(UserDetailsViewModel viewModel) {
            if (string.IsNullOrWhiteSpace(viewModel.FirstName))
                ModelState.AddModelError("FirstName", T("You must specify a first name."));

            if (string.IsNullOrWhiteSpace(viewModel.LastName))
                ModelState.AddModelError("LastName", T("You must specify a last name."));

            if (string.IsNullOrWhiteSpace(viewModel.Culture))
                ModelState.AddModelError("Culture", T("You must specify a language."));

            if (!ModelState.IsValid)
                return View();

            var user = _orchardServices.WorkContext.CurrentUser;
            _updateUserDetailsService.UpdateUserDetails(user, viewModel.FirstName, viewModel.LastName, viewModel.Culture, viewModel.ReceiveMails);
            _orchardServices.Notifier.Add(NotifyType.Success, T("Your details have been saved successfully."));
            return RedirectToAction("Index");
        }
    }
}