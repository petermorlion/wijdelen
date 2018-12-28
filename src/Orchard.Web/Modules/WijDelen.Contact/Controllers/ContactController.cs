using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;
using WijDelen.Contact.Models;
using WijDelen.Contact.Services;

namespace WijDelen.Contact.Controllers {
    [Themed]
    public class ContactController : Controller {
        private readonly IMailService _mailService;
        private readonly INotifier _notifier;
        private readonly IRecaptchaService _recaptchaService;

        private const string Pattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}$";

        public static int MaximumInputLength = 300;
        public static int MaximumTextAreaLength = 3000;

        public ContactController(IMailService mailService, INotifier notifier, IRecaptchaService recaptchaService) {
            _mailService = mailService;
            _notifier = notifier;
            _recaptchaService = recaptchaService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(ContactViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.Name))
                ModelState.AddModelError<ContactViewModel, string>(m => m.Name, T("Name is required."));
            else if (viewModel.Name.Length > MaximumInputLength)
                ModelState.AddModelError<ContactViewModel, string>(m => m.Name, T("Name is too long."));

            if (string.IsNullOrWhiteSpace(viewModel.Email))
                ModelState.AddModelError<ContactViewModel, string>(m => m.Email, T("Email is required."));
            else if (!Regex.IsMatch(viewModel.Email, Pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase))
                ModelState.AddModelError<ContactViewModel, string>(m => m.Email, T("Please provide a valid email address."));
            else if (viewModel.Email.Length > MaximumInputLength)
                ModelState.AddModelError<ContactViewModel, string>(m => m.Email, T("Email is too long."));

            if (string.IsNullOrWhiteSpace(viewModel.Subject))
                ModelState.AddModelError<ContactViewModel, string>(m => m.Subject, T("Subject is required."));
            else if (viewModel.Subject.Length > MaximumInputLength)
                ModelState.AddModelError<ContactViewModel, string>(m => m.Subject, T("Subject is too long."));

            if (string.IsNullOrWhiteSpace(viewModel.Text))
                ModelState.AddModelError<ContactViewModel, string>(m => m.Text, T("Text is required."));
            else if (viewModel.Text.Length > MaximumTextAreaLength)
                ModelState.AddModelError<ContactViewModel, string>(m => m.Text, T("Text is too long."));

            if (!_recaptchaService.Validates())
                ModelState.AddModelError<ContactViewModel, string>(m => m.Recaptcha, T("Please prove you are not a bot."));

            if (!ModelState.IsValid) {
                return View();
            }

            _mailService.SendContactMails(viewModel.Name, viewModel.Email, viewModel.Subject, viewModel.Text);
            _notifier.Add(NotifyType.Success, T("Thank you for your message. We will contact you as soon as possible."));
            return RedirectToAction("Index");
        }
    }
}