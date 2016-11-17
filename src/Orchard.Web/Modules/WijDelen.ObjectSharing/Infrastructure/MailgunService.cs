using System;
using System.Collections.Generic;
using Orchard.Localization;
using RestSharp;
using RestSharp.Authenticators;

namespace WijDelen.ObjectSharing.Infrastructure {
    public class MailgunService : IMailService {
        public MailgunService()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public void SendObjectRequestMail(string requestingUserName, string groupName, string description, string extraInfo, params string[] emailAddresses) {
            var client = new RestClient
            {
                BaseUrl = new Uri("https://api.mailgun.net/v3"),
                Authenticator = new HttpBasicAuthenticator("api", "key-9b8b2053d33de2583bfd3afb604dd820")
            };

            var request = new RestRequest();
            request.AddParameter("domain", "sandboxaa07be2124b6407f8b84a25c232b739c.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Mailgun Sandbox <postmaster@sandboxaa07be2124b6407f8b84a25c232b739c.mailgun.org>");

            var recipientVariables = new List<string>();

            foreach (var emailAddress in emailAddresses) {
                var yesLink = "";
                var notNowLink = "";
                var noLink = "";

                request.AddParameter("to", $"{emailAddress}");
                recipientVariables.Add($"\"{emailAddress}\": {{\"yesLink\":\"{yesLink}\", \"notNowLink\":\"{notNowLink}\", \"noLink\":\"{noLink}\"}}");
            }

            request.AddParameter("recipient-variables", $"{{{string.Join(",", recipientVariables)}}}");

            request.AddParameter("subject", T("Do you have a {0}?", description).ToString());
            request.AddParameter("text", T("object-request-mail-text", requestingUserName, groupName, description, extraInfo).ToString());
            request.AddParameter("html", T("object-request-mail-html", requestingUserName, groupName,description, extraInfo).ToString());

            request.Method = Method.POST;

            client.Execute(request);
        }
    }
}