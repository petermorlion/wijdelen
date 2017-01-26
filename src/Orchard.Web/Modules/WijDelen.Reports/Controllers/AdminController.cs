using System.Web.Mvc;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Controllers {
    public class AdminController : Controller {
        public ActionResult Index() {


            var viewModel = new DashboardViewModel {
                TotalUsers = 39,
                TotalGroups = 12,
                TotalObjectRequests = 885
            };

            return View(viewModel);
        }
    }
}