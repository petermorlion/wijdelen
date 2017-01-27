using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Localization;
using WijDelen.Reports.Queries;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Controllers {
    public class AdminController : Controller {
        private readonly ITotalsQuery _totalsQuery;
        private readonly IMonthSummaryQuery _monthSummaryQuery;
        private readonly IDateTimeProvider _dateTimeProvider;
        private IGroupMonthSummaryQuery _groupMonthSummaryQuery;

        public AdminController(ITotalsQuery totalsQuery, IMonthSummaryQuery monthSummaryQuery, IDateTimeProvider dateTimeProvider, IGroupMonthSummaryQuery groupMonthSummaryQuery) {
            _totalsQuery = totalsQuery;
            _monthSummaryQuery = monthSummaryQuery;
            _dateTimeProvider = dateTimeProvider;
            _groupMonthSummaryQuery = groupMonthSummaryQuery;
        }

        public ActionResult Index() {
            var totals = _totalsQuery.GetResults();

            var utcNow = _dateTimeProvider.UtcNow();
            var thisMonth = new DateTime(utcNow.Year, utcNow.Month, 1);
            var thisMonthSummary = _monthSummaryQuery.GetResults(thisMonth.Year, thisMonth.Month);

            var previousMonthYear = utcNow.Year;
            var previousMonthNumber = utcNow.Month - 1;
            if (previousMonthNumber <= 0) {
                previousMonthNumber = 12;
                previousMonthYear -= 1;
            }

            var previousMonth = new DateTime(previousMonthYear, previousMonthNumber, 1);
            var previousMonthSummary = _monthSummaryQuery.GetResults(previousMonth.Year, previousMonth.Month);

            var groupMonthSummary = _groupMonthSummaryQuery.GetResults(thisMonth.Year, thisMonth.Month).OrderByDescending(x => x.ObjectRequestCount).Take(10).ToList();

            var viewModel = new DashboardViewModel {
                TotalUsers = totals.Users,
                TotalGroups = totals.Groups,
                TotalObjectRequests = totals.ObjectRequests,
                ThisMonthSummary = thisMonthSummary,
                ThisMonth = thisMonth,
                PreviousMonthSummary = previousMonthSummary,
                PreviousMonth = previousMonth,
                GroupMonthSummaries = groupMonthSummary
            };

            return View(viewModel);
        }

        public Localizer T { get; set; }
    }
}