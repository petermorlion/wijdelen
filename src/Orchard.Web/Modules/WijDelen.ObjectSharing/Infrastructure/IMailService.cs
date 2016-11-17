namespace WijDelen.ObjectSharing.Infrastructure {
    public interface IMailService {
        void SendObjectRequestMail(string requestingUserName, string groupName, string description, string extraInfo, params string[] emailAddresses);
    }
}