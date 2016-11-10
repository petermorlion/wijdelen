using Orchard.Localization;

namespace WijDelen.ObjectSharing.Infrastructure {
    public class MailgunService : IMailService {
        public MailgunService()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public void SendObjectRequestMail(string description, string extraInfo, params string[] emailAddresses) {
            throw new System.NotImplementedException();
        }
    }
}