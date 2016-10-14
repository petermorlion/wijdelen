using Orchard.Localization;

namespace WijDelen.ObjectSharing.Infrastructure {
    public class MailgunService : IMailService {
        public MailgunService()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}