using System;
using System.Collections.Generic;
using Orchard;
using Orchard.Localization;
using RestSharp;
using RestSharp.Authenticators;
using WijDelen.ObjectSharing.Domain.Entities;

namespace WijDelen.ObjectSharing.Infrastructure {
    public class MailgunService : IMailService {
        private readonly IOrchardServices _orchardServices;

        public MailgunService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public void SendObjectRequestMail(string requestingUserName, string groupName, Guid objectRequestId, string description, string extraInfo, ObjectRequestMail objectRequestMail, params string[] emailAddresses) {
            var client = new RestClient
            {
                BaseUrl = new Uri("https://api.mailgun.net/v3"),
                Authenticator = new HttpBasicAuthenticator("api", "key-9b8b2053d33de2583bfd3afb604dd820")
            };

            var request = new RestRequest();
            request.AddParameter("domain", "sandboxaa07be2124b6407f8b84a25c232b739c.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Mailgun Sandbox <postmaster@sandboxaa07be2124b6407f8b84a25c232b739c.mailgun.org>");

            foreach (var emailAddress in emailAddresses) {
                request.AddParameter("to", $"{emailAddress}");
            }

            request.AddParameter("subject", T("Do you have a {0}?", description).ToString());

            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            var yesLink = siteUrl + "/WijDelen.ObjectSharing/ObjectRequestResponse/Confirm/" + objectRequestId;
            var notNowLink = siteUrl + "/WijDelen.ObjectSharing/ObjectRequestResponse/DenyForNow/" + objectRequestId;
            var noLink = siteUrl + "/WijDelen.ObjectSharing/ObjectRequestResponse/Deny/" + objectRequestId;

            var htmlBody = T("object-request-mail-html", requestingUserName, groupName, description, extraInfo, yesLink, notNowLink, noLink).ToString();
            request.AddParameter("text", T("object-request-mail-text", requestingUserName, groupName, description, extraInfo, yesLink, notNowLink, noLink).ToString());
            request.AddParameter("html", htmlBody);

            request.Method = Method.POST;

            client.Execute(request);

            objectRequestMail.MarkAsSent(emailAddresses, htmlBody);
        }
    }
}