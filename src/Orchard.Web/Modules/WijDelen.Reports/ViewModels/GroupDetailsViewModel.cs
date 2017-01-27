namespace WijDelen.Reports.ViewModels {
    public class GroupDetailsViewModel {
        public string GroupName { get; set; }
        public int RequestCount { get; set; }
        public int MailCount { get; set; }
        public int YesCount { get; set; }
        public int NoCount { get; set; }
        public int NotNowCount { get; set; }
    }
}