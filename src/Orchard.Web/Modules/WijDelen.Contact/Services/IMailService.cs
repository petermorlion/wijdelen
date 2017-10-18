using Orchard;

namespace WijDelen.Contact.Services {
    public interface IMailService : IDependency {
        /// <summary>
        /// Sends both a mail to the site admins as well as to the submitting user.
        /// </summary>
        void SendContactMails(string name, string email, string subject, string text);
    }
}