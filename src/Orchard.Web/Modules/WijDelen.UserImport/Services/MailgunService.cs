using System;
using System.Collections.Generic;
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

        public MailgunService(IUserService userService, IMailgunClient mailgunClient, IShapeFactory shapeFactory, IShapeDisplay shapeDisplay) {
            _userService = userService;
            _mailgunClient = mailgunClient;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void SendUserVerificationMails(IEnumerable<IUser> users, Func<string, string> createUrl) {
            var recipientVariables = new List<string>();
            var recipients = new List<string>();
            
            foreach (var user in users) {
                var nonce = _userService.CreateNonce(user, DelayToSetPassword);
                var url = createUrl(nonce);

                Logger.Log(LogLevel.Information, null, "Created nonce {0} for user {1}.", nonce, user.UserName);

                recipients.Add($"{user.UserName} <{user.Email}>");
                recipientVariables.Add($"\"{user.Email}\": {{\"username\":\"{user.UserName}\", \"loginlink\":\"{url}\"}}");
            }

            var recipientVariablesJson = $"{{{string.Join(",", recipientVariables)}}}";
            var subject = T("Welcome to Peergroups").ToString();
            
            var textShape = _shapeFactory.Create("Template_UserVerificationMail_Text");
            var htmlShape = _shapeFactory.Create("Template_UserVerificationMail");

            var text = _shapeDisplay.Display(textShape);
            var html = _shapeDisplay.Display(htmlShape);

            _mailgunClient.Send(recipients, recipientVariablesJson, subject, text, html);
        }
    }
}