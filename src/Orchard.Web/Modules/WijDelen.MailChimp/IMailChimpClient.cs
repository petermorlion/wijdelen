using Orchard;

namespace WijDelen.MailChimp {
    public interface IMailChimpClient : IDependency
    {
        bool IsSubscribed(string email);
        void Subscribe(string email);
        void Unsubscribe(string email);
    }
}