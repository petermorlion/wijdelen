using System.Web.Mvc;
using Orchard.Themes;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    public class ObjectRequestController : Controller {
        public ActionResult New() {
            return View();
        }
    }
}