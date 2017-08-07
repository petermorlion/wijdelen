using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Localization.Providers;
using Orchard.Mvc.Extensions;
using Orchard.Themes;
using Orchard.UI.Notify;
using WijDelen.MailChimp;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Controllers {
    [Themed]
    [Authorize]
    public class AccountController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IUpdateUserDetailsService _updateUserDetailsService;
        private readonly ICultureStorageProvider _cultureStorageProvider;
        private readonly IMailChimpClient _mailChimpClient;

        public AccountController(IOrchardServices orchardServices, 
            IUpdateUserDetailsService updateUserDetailsService, 
            ICultureStorageProvider cultureStorageProvider,
            IMailChimpClient mailChimpClient) {
            _orchardServices = orchardServices;
            _updateUserDetailsService = updateUserDetailsService;
            _cultureStorageProvider = cultureStorageProvider;
            _mailChimpClient = mailChimpClient;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var user = _orchardServices.WorkContext.CurrentUser;
            var userDetailsPart = user.ContentItem.As<UserDetailsPart>();
            var isSubscribedToNewsletter = _mailChimpClient.IsSubscribed(user.Email);
            var viewModel = new UserDetailsViewModel {
                FirstName = userDetailsPart.FirstName,
                LastName = userDetailsPart.LastName,
                Culture = userDetailsPart.Culture,
                ReceiveMails = userDetailsPart.ReceiveMails,
                IsSubscribedToNewsletter = isSubscribedToNewsletter
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
            _updateUserDetailsService.UpdateUserDetails(user, viewModel.FirstName, viewModel.LastName, viewModel.Culture, viewModel.ReceiveMails, viewModel.IsSubscribedToNewsletter);
            _orchardServices.WorkContext.CurrentCulture = viewModel.Culture;
            _cultureStorageProvider.SetCulture(viewModel.Culture);
            _orchardServices.Notifier.Add(NotifyType.Success, T("Your details have been saved successfully."));
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Unsubscribes from mails regarding object requests. Unsubscribing from newsletters will be handled
        /// in the user account details screen or via Mailchimp directly.
        /// </summary>
        public ActionResult Unsubscribe() {
            var user = _orchardServices.WorkContext.CurrentUser;
            var userDetailsPart = user.As<UserDetailsPart>();
            _updateUserDetailsService.UpdateUserDetails(user, userDetailsPart.FirstName, userDetailsPart.LastName, userDetailsPart.Culture, false);
            _orchardServices.Notifier.Add(NotifyType.Success, T("You will no longer receive mails regarding requests or chat messages."));

            return RedirectToAction("Index");
        }
    }
}