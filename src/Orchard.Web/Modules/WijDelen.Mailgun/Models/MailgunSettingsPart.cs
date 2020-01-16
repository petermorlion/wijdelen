using Orchard.ContentManagement;

namespace WijDelen.Mailgun.Models {
    public class MailgunSettingsPart : ContentPart {
        public string ApiKey {
            get { return this.Retrieve(x => x.ApiKey); }
            set { this.Store(x => x.ApiKey, value); }
        }

        public string ApiBaseUrl
        {
            get { return this.Retrieve(x => x.ApiBaseUrl); }
            set { this.Store(x => x.ApiBaseUrl, value); }
        }

        public string Domain
        {
            get { return this.Retrieve(x => x.Domain); }
            set { this.Store(x => x.Domain, value); }
        }

        public string To
        {
            get { return this.Retrieve(x => x.To); }
            set { this.Store(x => x.To, value); }
        }

        public string From
        {
            get { return this.Retrieve(x => x.From); }
            set { this.Store(x => x.From, value); }
        }
    }
}