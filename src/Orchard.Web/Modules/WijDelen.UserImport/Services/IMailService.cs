using Orchard;

namespace WijDelen.UserImport.Services {
    public interface IMailService : IDependency {
        void SendUserVerificationMail(string userName);
    }
}