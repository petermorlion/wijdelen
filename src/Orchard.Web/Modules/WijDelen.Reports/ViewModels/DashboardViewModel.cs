namespace WijDelen.Reports.ViewModels {
    public class DashboardViewModel {
        public int TotalUsers { get; set; }
        public int TotalGroups { get; set; }
        public int TotalObjectRequests { get; set; }
        public SummaryViewModel ThisMonthSummary { get; set; }
        public SummaryViewModel PreviousMonthSummary { get; set; }
    }
}