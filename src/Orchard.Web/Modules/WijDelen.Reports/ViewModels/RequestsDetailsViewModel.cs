using System;

namespace WijDelen.Reports.ViewModels {
    public class RequestsDetailsViewModel {
        public DateTime CreatedDateTime { get; set; }
        public string GroupName { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// How many mails were sent out for this request.
        /// </summary>
        public int MailCount { get; set; }
        public int YesCount { get; set; }
        public int NoCount { get; set; }
        public int NotNowCount { get; set; }
    }
}