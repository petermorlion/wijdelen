using System;
using System.Collections.Generic;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Services;
using WijDelen.Mailgun;

namespace WijDelen.UserImport.Services {
    public class MailgunService : IMailService {
        private static readonly TimeSpan DelayToSetPassword = TimeSpan.FromDays(60);

        private readonly IUserService _userService;
        private readonly IMailgunClient _mailgunClient;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly IOrchardServices _orchardServices;

        public MailgunService(IUserService userService, IMailgunClient mailgunClient, IShapeFactory shapeFactory, IShapeDisplay shapeDisplay, IOrchardServices orchardServices) {
            _userService = userService;
            _mailgunClient = mailgunClient;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void SendUserInvitationMails(string culture, IEnumerable<IUser> users, Func<string, string> createUrl, string groupName, string groupLogoUrl, string extraInfoHtml) {
            var originalCulture = _orchardServices.WorkContext.CurrentCulture;

            try {
                if (!string.IsNullOrEmpty(culture))
                    _orchardServices.WorkContext.CurrentCulture = culture;

                var recipientVariables = new List<string>();
                var recipients = new List<string>();

                foreach (var user in users)
                {
                    var nonce = _userService.CreateNonce(user, DelayToSetPassword);
                    var url = createUrl(nonce);

                    Logger.Log(LogLevel.Information, null, "Created nonce {0} for user {1}.", nonce, user.UserName);

                    recipients.Add($"{user.UserName} <{user.Email}>");
                    recipientVariables.Add($"\"{user.Email}\": {{\"username\":\"{user.UserName}\", \"loginlink\":\"{url}\"}}");
                }

                var recipientVariablesJson = $"{{{string.Join(",", recipientVariables)}}}";
                var subject = T("Welcome to Peergroups").ToString();

                var textShape = _shapeFactory.Create("Template_UserInvitationMail_Text", Arguments.From(new
                {
                    GroupName = groupName
                }));

                var currentUrl = _orchardServices.WorkContext.HttpContext.Request.Url;
                var applicationPath = _orchardServices.WorkContext.HttpContext.Request.ApplicationPath;
                var baseUrl = currentUrl.Scheme + "://" + currentUrl.Authority + applicationPath.TrimEnd('/') + "/";
                var htmlShape = _shapeFactory.Create("Template_UserInvitationMail", Arguments.From(new
                {
                    GroupName = groupName,
                    GroupLogoUrl = groupLogoUrl,
                    PeergroupsLogoUrl = baseUrl + "/Themes/Peergroups.Theme/Content/logo-mail.gif",
                    ExtraInfoHtml = extraInfoHtml
                }));

                var text = _shapeDisplay.Display(textShape);
                var html = _shapeDisplay.Display(htmlShape);

                _mailgunClient.Send(recipients, recipientVariablesJson, subject, text, html);
            }
            finally {
                _orchardServices.WorkContext.CurrentCulture = originalCulture;
            }
        }
    }
}