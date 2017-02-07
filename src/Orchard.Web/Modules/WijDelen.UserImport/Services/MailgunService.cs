using System;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Users.Services;
using WijDelen.Mailgun;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services {
    public class MailgunService : IMailService {
        private static readonly TimeSpan DelayToSetPassword = TimeSpan.FromDays(7);

        private readonly IUserService _userService;
        private readonly IMailgunClient _mailgunClient;

        public MailgunService(IUserService userService, IMailgunClient mailgunClient) {
            _userService = userService;
            _mailgunClient = mailgunClient;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void SendUserVerificationMails(IEnumerable<UserImportResult> userImportResults, Func<string, string> createUrl) {
            var recipientVariables = new List<string>();
            var recipients = new List<string>();
            
            foreach (var userImportResult in userImportResults) {
                var nonce = _userService.CreateNonce(userImportResult.User, DelayToSetPassword);
                var url = createUrl(nonce);

                recipients.Add($"{userImportResult.UserName} <{userImportResult.Email}>");
                recipientVariables.Add($"\"{userImportResult.Email}\": {{\"username\":\"{userImportResult.UserName}\", \"loginlink\":\"{url}\"}}");
            }

            var recipientVariablesJson = $"{{{string.Join(",", recipientVariables)}}}";
            var subject = T("Welcome to WijDelen Groups").ToString();
            var text = T("user-verification-mail-text").ToString();
            var html = T("user-verification-mail-html").ToString();

            _mailgunClient.Send(recipients, recipientVariablesJson, subject, text, html);
        }
    }
}