using System.Web.Mvc;
using Orchard.Themes;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    public class ObjectRequestController : Controller {
        public ActionResult New() {
            return View(new NewObjectRequestViewModel());
        }

        [HttpPost]
        public ActionResult New(NewObjectRequestViewModel viewModel) {
            return View(viewModel);
        }
    }
}