using System;
using System.Collections.Generic;
using System.Net;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RestSharp;
using RestSharp.Authenticators;
using WijDelen.Mailgun.Models;

namespace WijDelen.Mailgun {
    public class MailgunClient : IMailgunClient {
        private readonly IOrchardServices _orchardServices;

        public MailgunClient(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        /// <summary>
        /// Creates an initial authenticated POST request for Mailgun, including the From address. Anything
        /// else should be added (to, subject, text, html).
        /// </summary>
        /// <param name="recipients">A list of recipients, usually in the form "Name &lt;Email&gt;".</param>
        /// <param name="recipientVariables">Recipient variables, as documented by Mailgun. Pass in an empty string if there are none.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlMail">A html version of the mail.</param>
        /// <param name="replyTo">Specify a different reply address, if necessary.</param>
        public void Send(IEnumerable<string> recipients, string recipientVariables, string subject, string htmlMail, string replyTo = "") {
            var settings = _orchardServices.WorkContext.CurrentSite.As<MailgunSettingsPart>();
            var client = new RestClient {
                BaseUrl = new Uri(settings.ApiBaseUrl),
                Authenticator = new HttpBasicAuthenticator("api", settings.ApiKey)
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var request = new RestRequest();
            request.AddParameter("domain", settings.Domain, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", settings.From);
            request.AddParameter("subject", subject);
            request.AddParameter("html", htmlMail);
            request.AddParameter("to", settings.To);

            if (!string.IsNullOrEmpty(replyTo)) {
                request.AddParameter("h:Reply-To", replyTo);
            }

            foreach (var recipient in recipients) {
                request.AddParameter("bcc", recipient);
            }

            if (!string.IsNullOrEmpty(recipientVariables)) {
                request.AddParameter("recipient-variables", recipientVariables);
            }

            request.Method = Method.POST;

            var response = client.Execute(request);

            var statusCodeNumber = (int) response.StatusCode;
            if (statusCodeNumber < 200 || statusCodeNumber > 399) {
                Logger.Error("An error occurred when calling Mailgun. Received a {0} and the following content: {1}. ErrorMessage: {2}. ErrorException: {3}", response.StatusCode, response.Content, response.ErrorMessage, response.ErrorException);
            }
        }

        public ILogger Logger { get; set; }
    }
}
