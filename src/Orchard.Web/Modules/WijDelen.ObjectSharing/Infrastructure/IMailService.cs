namespace WijDelen.ObjectSharing.Infrastructure {
    public interface IMailService {
        void SendObjectRequestMail(string description, string extraInfo, params string[] emailAddresses);
    }
}