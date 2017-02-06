using System;
using System.Collections.Generic;
using RestSharp;
using RestSharp.Authenticators;

namespace WijDelen.Mailgun {
    public class MailgunClient :
        ObjectSharing.Infrastructure.Factories.IMailgunClient,
        UserImport.Factories.IMailgunClient {
        /// <summary>
        /// Creates an initial authenticated POST request for Mailgun, including the From address. Anything
        /// else should be added (to, subject, text, html).
        /// </summary>
        /// <param name="recipients">A list of recipients, usually in the form "Name &lt;Email&gt;".</param>
        /// <param name="recipientVariables">Recipient variables, as documented by Mailgun. Pass in an empty string if there are none.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="textMail">A text version of the mail.</param>
        /// <param name="htmlMail">A html version of the mail.</param>
        public void Send(IEnumerable<string> recipients, string recipientVariables, string subject, string textMail, string htmlMail) {
            var client = new RestClient {
                BaseUrl = new Uri("https://api.mailgun.net/v3"),
                Authenticator = new HttpBasicAuthenticator("api", "key-9b8b2053d33de2583bfd3afb604dd820")
            };

            var request = new RestRequest();
            request.AddParameter("domain", "sandboxaa07be2124b6407f8b84a25c232b739c.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Mailgun Sandbox <postmaster@sandboxaa07be2124b6407f8b84a25c232b739c.mailgun.org>");
            request.AddParameter("subject", subject);
            request.AddParameter("text", textMail);
            request.AddParameter("html", htmlMail);

            foreach (var recipient in recipients) {
                request.AddParameter("to", recipient);
            }

            if (!string.IsNullOrEmpty(recipientVariables)) {
                request.AddParameter("recipient-variables", recipientVariables);
            }

            request.Method = Method.POST;

            client.Execute(request);
        }
    }
}