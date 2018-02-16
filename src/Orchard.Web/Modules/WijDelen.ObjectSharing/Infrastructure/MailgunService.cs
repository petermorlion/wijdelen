using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.MediaLibrary.Fields;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Users.Models;
using WijDelen.Mailgun;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Infrastructure {
    public class MailgunService : IMailService {
        private readonly IMailgunClient _mailgunClient;
        private readonly IOrchardServices _orchardServices;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly IShapeFactory _shapeFactory;

        public MailgunService(IOrchardServices orchardServices, IMailgunClient mailgunClient, IShapeFactory shapeFactory, IShapeDisplay shapeDisplay) {
            _orchardServices = orchardServices;
            _mailgunClient = mailgunClient;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void SendObjectRequestMail(string requestingUserName, string groupName, Guid objectRequestId, string description, string extraInfo, ObjectRequestMail objectRequestMail, IEnumerable<IUser> users) {
            var usersByCulture = users.GroupBy(x => x.As<UserDetailsPart>()?.Culture);
            foreach (var usersByCultureGroup in usersByCulture)
                SendLocalizedObjectRequestMail(usersByCultureGroup.Key, requestingUserName, groupName, objectRequestId, description, extraInfo, objectRequestMail, usersByCultureGroup.ToArray());
        }

        public void SendChatMessageAddedMail(string culture, string fromUserName, string description, string toEmailAddress, Guid chatId, string message) {
            var originalCulture = _orchardServices.WorkContext.CurrentCulture;

            try {
                if (!string.IsNullOrEmpty(culture))
                    _orchardServices.WorkContext.CurrentCulture = culture;

                var subject = T("{0} reacted on your request for a {1}.", fromUserName, description).ToString();
                var to = new[] {toEmailAddress};

                var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
                var chatUrl = siteUrl + "/WijDelen.ObjectSharing/Chat/Index/" + chatId;
                var unsubscribeUrl = siteUrl + "/WijDelen.UserImport/Account/Unsubscribe";

                var groupLogoUrl = "";

                if (_orchardServices.WorkContext.CurrentUser != null) {
                    var user = _orchardServices.WorkContext.CurrentUser;
                    var groupMembership = user.As<GroupMembershipPart>();
                    var group = groupMembership.Group;
                    var groupLogoPart = group.ContentItem.Parts.Single(x => x.PartDefinition.Name == "GroupLogoPart");
                    var groupLogoField = groupLogoPart.Fields.Single(x => x.FieldDefinition.Name == "MediaLibraryPickerField") as MediaLibraryPickerField;

                    if (!string.IsNullOrEmpty(groupLogoField?.FirstMediaUrl)) groupLogoUrl = siteUrl + groupLogoField.FirstMediaUrl;
                }

                var textShape = _shapeFactory.Create("Template_ChatMessageAddedMail_Text", Arguments.From(new {
                    FromUserName = fromUserName,
                    Description = description,
                    Message = message,
                    ChatUrl = chatUrl,
                    UnsubscribeUrl = unsubscribeUrl
                }));

                var htmlShape = _shapeFactory.Create("Template_ChatMessageAddedMail", Arguments.From(new {
                    GroupLogoUrl = groupLogoUrl,
                    FromUserName = fromUserName,
                    Description = description,
                    Message = message,
                    ChatUrl = chatUrl,
                    UnsubscribeUrl = unsubscribeUrl
                }));

                var text = _shapeDisplay.Display(textShape);
                var html = _shapeDisplay.Display(htmlShape);

                _mailgunClient.Send(to, "", subject, text, html);
            }
            finally {
                _orchardServices.WorkContext.CurrentCulture = originalCulture;
            }
        }

        public void SendAdminObjectRequestBlockedMail(Guid objectRequestId, string requestingUserName, string description, string extraInfo, IList<string> forbiddenWords) {
            var htmlShape = _shapeFactory.Create("Template_AdminObjectRequestBlockedMail", Arguments.From(new {
                RequestingUserName = requestingUserName,
                Description = description,
                ExtraInfo = extraInfo,
                ForbiddenWords = forbiddenWords,
                ObjectRequestId = objectRequestId
            }));

            var subject = T("A request for (a) {0} was blocked", description).ToString();
            var html = _shapeDisplay.Display(htmlShape);

            var userParts = _orchardServices.ContentManager.Query<UserPart>().List();
            var admins = userParts.Where(x => x.As<UserRolesPart>().Roles.Contains("PeergroupsAdministrator")).ToList();
            var adminEmails = admins.Select(x => x.Email).ToList();

            _mailgunClient.Send(adminEmails, "", subject, "", html);
        }

        public void SendAdminObjectRequestMail(string requestingUserName, string description, string extraInfo) {
            var htmlShape = _shapeFactory.Create("Template_AdminObjectRequestMail", Arguments.From(new {
                RequestingUserName = requestingUserName,
                Description = description,
                ExtraInfo = extraInfo
            }));

            var subject = T("A request for (a) {0} was made", description).ToString();
            var html = _shapeDisplay.Display(htmlShape);

            var userParts = _orchardServices.ContentManager.Query<UserPart>().List();
            var admins = userParts.Where(x => x.As<UserRolesPart>().Roles.Contains("PeergroupsAdministrator")).ToList();
            var adminEmails = admins.Select(x => x.Email).ToList();

            _mailgunClient.Send(adminEmails, "", subject, "", html);
        }

        private void SendLocalizedObjectRequestMail(string culture, string requestingUserName, string groupName, Guid objectRequestId, string description, string extraInfo, ObjectRequestMail objectRequestMail, params IUser[] users) {
            var originalCulture = _orchardServices.WorkContext.CurrentCulture;

            try {
                if (!string.IsNullOrEmpty(culture))
                    _orchardServices.WorkContext.CurrentCulture = culture;

                var recipients = new List<string>();
                var userEmails = users.Select(x => new UserEmail {UserId = x.Id, Email = x.Email}).ToList();

                foreach (var userEmail in userEmails)
                    recipients.Add(userEmail.Email);

                var subject = T("Do you have (a) {0}?", description).ToString();

                var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
                var yesLink = siteUrl + "/WijDelen.ObjectSharing/ObjectRequestResponse/Confirm/" + objectRequestId;
                var notNowLink = siteUrl + "/WijDelen.ObjectSharing/ObjectRequestResponse/DenyForNow/" + objectRequestId;
                var noLink = siteUrl + "/WijDelen.ObjectSharing/ObjectRequestResponse/Deny/" + objectRequestId;

                var groupLogoUrl = "";
                var unsubscribeUrl = siteUrl + "/WijDelen.UserImport/Account/Unsubscribe";

                if (_orchardServices.WorkContext.CurrentUser != null) {
                    var user = _orchardServices.WorkContext.CurrentUser;
                    var groupMembership = user.As<GroupMembershipPart>();
                    var group = groupMembership.Group;
                    var groupLogoPart = group.ContentItem.Parts.Single(x => x.PartDefinition.Name == "GroupLogoPart");
                    var groupLogoField = groupLogoPart.Fields.Single(x => x.FieldDefinition.Name == "MediaLibraryPickerField") as MediaLibraryPickerField;

                    if (!string.IsNullOrEmpty(groupLogoField?.FirstMediaUrl)) groupLogoUrl = siteUrl + groupLogoField.FirstMediaUrl;
                }

                var textShape = _shapeFactory.Create("Template_ObjectRequestMail_Text", Arguments.From(new {
                    RequestingUserName = requestingUserName,
                    GroupName = groupName,
                    Description = description,
                    ExtraInfo = extraInfo,
                    YesLink = yesLink,
                    NotNowLink = notNowLink,
                    NoLink = noLink,
                    UnsubscribeUrl = unsubscribeUrl
                }));

                var htmlShape = _shapeFactory.Create("Template_ObjectRequestMail", Arguments.From(new {
                    GroupLogoUrl = groupLogoUrl,
                    RequestingUserName = requestingUserName,
                    GroupName = groupName,
                    Description = description,
                    ExtraInfo = extraInfo,
                    YesLink = yesLink,
                    NotNowLink = notNowLink,
                    NoLink = noLink,
                    UnsubscribeUrl = unsubscribeUrl
                }));

                var text = _shapeDisplay.Display(textShape);
                var html = _shapeDisplay.Display(htmlShape);

                _mailgunClient.Send(recipients, "", subject, text, html);

                objectRequestMail.MarkAsSent(userEmails, html);
            }
            finally {
                _orchardServices.WorkContext.CurrentCulture = originalCulture;
            }
        }

        public void SendObjectRequestBlockedMail(IUser requestingUser, Guid sourceId, string description, string reason) {
            var originalCulture = _orchardServices.WorkContext.CurrentCulture;
            var culture = requestingUser.As<UserDetailsPart>()?.Culture;

            try
            {
                if (!string.IsNullOrEmpty(culture))
                    _orchardServices.WorkContext.CurrentCulture = culture;

                var recipients = new List<string> { requestingUser.Email };

                var subject = T("Your request has been blocked", description).ToString();

                var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
                var objectRequestLink = siteUrl + "/WijDelen.ObjectSharing/ObjectRequest/Item/" + sourceId;

                var groupLogoUrl = "";
                var unsubscribeUrl = siteUrl + "/WijDelen.UserImport/Account/Unsubscribe";

                if (_orchardServices.WorkContext.CurrentUser != null)
                {
                    var user = _orchardServices.WorkContext.CurrentUser;
                    var groupMembership = user.As<GroupMembershipPart>();
                    var group = groupMembership.Group;
                    var groupLogoPart = group.ContentItem.Parts.Single(x => x.PartDefinition.Name == "GroupLogoPart");
                    var groupLogoField = groupLogoPart.Fields.Single(x => x.FieldDefinition.Name == "MediaLibraryPickerField") as MediaLibraryPickerField;

                    if (!string.IsNullOrEmpty(groupLogoField?.FirstMediaUrl)) groupLogoUrl = siteUrl + groupLogoField.FirstMediaUrl;
                }

                var htmlShape = _shapeFactory.Create("Template_ObjectRequestBlockedMail", Arguments.From(new
                {
                    GroupLogoUrl = groupLogoUrl,
                    Description = description,
                    ObjectRequestLink = objectRequestLink,
                    UnsubscribeUrl = unsubscribeUrl,
                    Reason = reason
                }));

                var html = _shapeDisplay.Display(htmlShape);

                _mailgunClient.Send(recipients, "", subject, "", html, "info@peergroups.be");
            }
            finally
            {
                _orchardServices.WorkContext.CurrentCulture = originalCulture;
            }
        }
    }
}