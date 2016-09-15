using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Themes;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    public class ObjectRequestController : Controller {
        public ObjectRequestController() {
            T = NullLocalizer.Instance;
        }

        public ActionResult New() {
            return View(new NewObjectRequestViewModel());
        }

        [HttpPost]
        public ActionResult New(NewObjectRequestViewModel viewModel) {
            if (string.IsNullOrWhiteSpace(viewModel.Description)) {
                ModelState.AddModelError("Description", T("Please provide a description of the item you need."));
            }

            if (string.IsNullOrWhiteSpace(viewModel.ExtraInfo))
            {
                ModelState.AddModelError("ExtraInfo", T("Please provide some extra info."));
            }

            return View(viewModel);
        }

        public Localizer T { get; set; }
    }
}