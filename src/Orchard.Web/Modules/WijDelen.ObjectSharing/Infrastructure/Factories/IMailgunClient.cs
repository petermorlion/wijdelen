using System.Collections.Generic;
using Orchard;

namespace WijDelen.ObjectSharing.Infrastructure.Factories {
    public interface IMailgunClient : IDependency {
        /// <summary>
        /// Creates an initial authenticated POST request for Mailgun, including the From address. Anything
        /// else should be added (to, subject, text, html).
        /// </summary>
        /// <param name="recipients">A list of recipients, usually in the form "Name &lt;Email&gt;".</param>
        /// <param name="recipientVariables">Recipient variables, as documented by Mailgun. Pass in an empty string if there are none.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="textMail">A text version of the mail.</param>
        /// <param name="htmlMail">A html version of the mail.</param>
        void Send(IEnumerable<string> recipients, string recipientVariables, string subject, string textMail, string htmlMail);
    }
}