using System;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Users.Services;
using RestSharp;
using RestSharp.Authenticators;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services {
    public class MailgunService : IMailService {
        private static readonly TimeSpan DelayToSetPassword = TimeSpan.FromDays(7);

        private readonly IUserService _userService;

        public MailgunService(IUserService userService) {
            _userService = userService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void SendUserVerificationMails(IEnumerable<UserImportResult> userImportResults, Func<string, string> createUrl) {
            var client = new RestClient {
                BaseUrl = new Uri("https://api.mailgun.net/v3"),
                Authenticator = new HttpBasicAuthenticator("api", "key-9b8b2053d33de2583bfd3afb604dd820")
            };

            var request = new RestRequest();
            request.AddParameter("domain", "sandboxaa07be2124b6407f8b84a25c232b739c.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Mailgun Sandbox <postmaster@sandboxaa07be2124b6407f8b84a25c232b739c.mailgun.org>");

            var recipientVariables = new List<string>();
            
            foreach (var userImportResult in userImportResults) {
                var nonce = _userService.CreateNonce(userImportResult.User, DelayToSetPassword);
                var url = createUrl(nonce);

                request.AddParameter("to", $"{userImportResult.UserName} <{userImportResult.Email}>");
                recipientVariables.Add($"\"{userImportResult.Email}\": {{\"username\":\"{userImportResult.UserName}\", \"loginlink\":\"{url}\"}}");
            }
            
            request.AddParameter("recipient-variables", $"{{{string.Join(",", recipientVariables)}}}");

            request.AddParameter("subject", T("Welcome to WijDelen Groups").ToString());
            request.AddParameter("text", T("user-verification-mail-text").ToString());
            request.AddParameter("html", T("user-verification-mail-html").ToString());
            request.Method = Method.POST;

            client.Execute(request);
        }
    }
}