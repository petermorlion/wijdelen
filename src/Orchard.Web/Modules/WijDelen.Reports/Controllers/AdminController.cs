using System;
using System.Web.Mvc;
using Orchard.Localization;
using WijDelen.Reports.Queries;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Controllers {
    public class AdminController : Controller {
        private readonly ITotalsQuery _totalsQuery;
        private readonly IMonthSummaryQuery _monthSummaryQuery;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AdminController(ITotalsQuery totalsQuery, IMonthSummaryQuery monthSummaryQuery, IDateTimeProvider dateTimeProvider) {
            _totalsQuery = totalsQuery;
            _monthSummaryQuery = monthSummaryQuery;
            _dateTimeProvider = dateTimeProvider;
        }

        public ActionResult Index() {
            var totals = _totalsQuery.GetResults();
            var thisMonthSummary = _monthSummaryQuery.GetResults(_dateTimeProvider.UtcNow().Month);

            var previousMonth = _dateTimeProvider.UtcNow().Month - 1;
            if (previousMonth <= 0)
                previousMonth = 12;

            var previousMonthSummary = _monthSummaryQuery.GetResults(previousMonth);

            var viewModel = new DashboardViewModel {
                TotalUsers = totals.Users,
                TotalGroups = totals.Groups,
                TotalObjectRequests = totals.ObjectRequests,
                ThisMonthSummary = thisMonthSummary,
                PreviousMonthSummary = previousMonthSummary
            };

            return View(viewModel);
        }

        public Localizer T { get; set; }
    }
}