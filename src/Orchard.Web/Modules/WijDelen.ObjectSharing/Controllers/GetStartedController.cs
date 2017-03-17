using System.Web.Mvc;
using Orchard.Themes;
using Orchard.Localization;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    [Authorize]
    public class GetStartedController : Controller {
        public ActionResult Index(string nonce) {
            return View();
        }
    }
}