using System;
using System.Collections.Generic;
using Orchard.Logging;
using RestSharp;
using RestSharp.Authenticators;

namespace WijDelen.Mailgun {
    public class MailgunClient : IMailgunClient {
        private readonly Uri _apiBaseUrl;
        private readonly string _apiKey;
        private readonly string _domain;
        private readonly string _from;

        public MailgunClient() {
            Logger = NullLogger.Instance;

            _apiKey = "key-9b8b2053d33de2583bfd3afb604dd820";

#if DEBUG
            _apiBaseUrl = new Uri("https://api.mailgun.net/v3");
            _domain = "sandboxaa07be2124b6407f8b84a25c232b739c.mailgun.org";
            _from = "Mailgun Sandbox <postmaster@sandboxaa07be2124b6407f8b84a25c232b739c.mailgun.org>";
#else
            _apiBaseUrl = new Uri("https://api.mailgun.net/v3");
            _domain = "mg.peergroups.be";
            _from = "Peergroups <no-reply@peergroups.be>";
#endif
        }

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
                BaseUrl = _apiBaseUrl,
                Authenticator = new HttpBasicAuthenticator("api", _apiKey)
            };

            var request = new RestRequest();
            request.AddParameter("domain", _domain, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", _from);
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

            var response = client.Execute(request);

            var statusCodeNumber = (int) response.StatusCode;
            if (statusCodeNumber < 200 || statusCodeNumber > 399) {
                Logger.Error("An error occurred when calling Mailgun. Received a {0} and the following content: {1}.", response.StatusCode, response.Content);
            }
        }

        public ILogger Logger { get; set; }
    }
}