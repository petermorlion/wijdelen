using System.Web.Mvc;
using Orchard.UI.Admin;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ObjectRequestAdminController : Controller {
        public ActionResult Index() {
            return View();
        }
    }
}