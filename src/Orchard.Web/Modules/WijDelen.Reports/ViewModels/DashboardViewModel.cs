using System;
using System.Collections.Generic;

namespace WijDelen.Reports.ViewModels {
    public class DashboardViewModel {
        public int TotalUsers { get; set; }
        public int TotalGroups { get; set; }
        public int TotalObjectRequests { get; set; }
        public SummaryViewModel ThisMonthSummary { get; set; }
        public SummaryViewModel PreviousMonthSummary { get; set; }
        public DateTime ThisMonth { get; set; }
        public DateTime PreviousMonth { get; set; }
        public IList<GroupMonthSummaryViewModel> GroupMonthSummaries { get; set; }
    }
}