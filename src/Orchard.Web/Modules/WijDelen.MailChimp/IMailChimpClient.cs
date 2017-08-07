using Orchard;

namespace WijDelen.MailChimp {
    public interface IMailChimpClient : IDependency
    {
        bool IsSubscribed(string email);
        void Subscribe(string email, string firstName, string lastName);
        void Unsubscribe(string email, string firstName, string lastName);
    }
}