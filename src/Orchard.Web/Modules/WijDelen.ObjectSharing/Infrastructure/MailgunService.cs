using System;
using System.Collections.Generic;
using Orchard;
using Orchard.Localization;
using WijDelen.Mailgun;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.ValueTypes;

namespace WijDelen.ObjectSharing.Infrastructure {
    public class MailgunService : IMailService {
        private readonly IOrchardServices _orchardServices;
        private readonly IMailgunClient _mailgunClient;

        public MailgunService(IOrchardServices orchardServices, IMailgunClient mailgunClient) {
            _orchardServices = orchardServices;
            _mailgunClient = mailgunClient;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public void SendObjectRequestMail(string requestingUserName, string groupName, Guid objectRequestId, string description, string extraInfo, ObjectRequestMail objectRequestMail, params UserEmail[] userEmails) {
            var recipients = new List<string>();
            foreach (var userEmail in userEmails) {
                recipients.Add(userEmail.Email);
            }

            var subject = T("Do you have (a) {0}?", description).ToString();

            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            var yesLink = siteUrl + "/WijDelen.ObjectSharing/ObjectRequestResponse/Confirm/" + objectRequestId;
            var notNowLink = siteUrl + "/WijDelen.ObjectSharing/ObjectRequestResponse/DenyForNow/" + objectRequestId;
            var noLink = siteUrl + "/WijDelen.ObjectSharing/ObjectRequestResponse/Deny/" + objectRequestId;

            var htmlBody = T("object-request-mail-html", requestingUserName, groupName, description, extraInfo, yesLink, notNowLink, noLink).ToString();
            var textBody = T("object-request-mail-text", requestingUserName, groupName, description, extraInfo, yesLink, notNowLink, noLink).ToString();

            _mailgunClient.Send(recipients, "", subject, textBody, htmlBody);

            objectRequestMail.MarkAsSent(userEmails, htmlBody);
        }

        public void SendChatMessageAddedMail(string fromUserName, string toUserName, string description, string toEmailAddress, Guid chatId, string message) {
            var subject = T("{0} reacted on your request for a {1}.", fromUserName, description).ToString();
            var to = new[] {toEmailAddress};

            var chatUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl + "/WijDelen.ObjectSharing/Chat/Index/" + chatId;
            
            var htmlBody = T("chat-message-added-mail-html", fromUserName, description, message, chatUrl).ToString();
            var textBody = T("chat-message-added-mail-text", fromUserName, description, message, chatUrl).ToString();

            _mailgunClient.Send(to, "", subject, textBody, htmlBody);
        }
    }
}