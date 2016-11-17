using Orchard;

namespace WijDelen.ObjectSharing.Infrastructure {
    public interface IMailService : IDependency {
        void SendObjectRequestMail(string requestingUserName, string groupName, string description, string extraInfo, params string[] emailAddresses);
    }
}