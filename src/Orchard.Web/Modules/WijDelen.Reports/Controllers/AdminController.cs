using System.Web.Mvc;
using Orchard.Localization;
using WijDelen.Reports.Queries;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Controllers {
    public class AdminController : Controller {
        private readonly ITotalsQuery _totalsQuery;

        public AdminController(ITotalsQuery totalsQuery) {
            _totalsQuery = totalsQuery;
        }

        public ActionResult Index() {
            var totals = _totalsQuery.GetResults();

            var viewModel = new DashboardViewModel {
                TotalUsers = totals.Users,
                TotalGroups = totals.Groups,
                TotalObjectRequests = totals.ObjectRequests
            };

            return View(viewModel);
        }

        public Localizer T { get; set; }
    }
}