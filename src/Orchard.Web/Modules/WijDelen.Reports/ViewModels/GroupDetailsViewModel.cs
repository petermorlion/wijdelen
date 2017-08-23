namespace WijDelen.Reports.ViewModels {
    public class GroupDetailsViewModel {
        public string GroupName { get; set; }

        /// <summary>
        /// How many requests were made in this group.
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// How many mails were sent out in this group.
        /// </summary>
        public int MailCount { get; set; }
        public int YesCount { get; set; }
        public int NoCount { get; set; }
        public int NotNowCount { get; set; }
    }
}