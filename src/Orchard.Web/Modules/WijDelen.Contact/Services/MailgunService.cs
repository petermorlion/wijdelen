using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Users.Models;
using WijDelen.Mailgun;

namespace WijDelen.Contact.Services
{
    public class MailgunService : IMailService {
        private readonly IOrchardServices _orchardServices;
        private readonly IMailgunClient _mailgunClient;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;

        public MailgunService(IOrchardServices orchardServices, IMailgunClient mailgunClient, IShapeFactory shapeFactory, IShapeDisplay shapeDisplay) {
            _orchardServices = orchardServices;
            _mailgunClient = mailgunClient;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void SendContactMails(string name, string email, string subject, string text) {
            SendMailToAdmin(name, email, subject, text);
            SendConfirmationMail(name, email, subject, text);
        }

        private void SendConfirmationMail(string name, string email, string subject, string text) {
            var htmlShape = _shapeFactory.Create("Template_ContactMail", Arguments.From(new
            {
                Name = name,
                Email = email,
                Subject = subject,
                Text = text
            }));

            var mailSubject = T("Thank you for contacting Peergroups", text).ToString();
            var html = _shapeDisplay.Display(htmlShape);

            _mailgunClient.Send(new List<string> {email}, "", mailSubject, "", html);
        }

        private void SendMailToAdmin(string name, string email, string subject, string text) {
            var htmlShape = _shapeFactory.Create("Template_AdminContactMail", Arguments.From(new
            {
                Name = name,
                Email = email,
                Subject = subject,
                Text = text
            }));

            var mailSubject = T("Contact form submitted: {0}", subject).ToString();
            var html = _shapeDisplay.Display(htmlShape);

            var userParts = _orchardServices.ContentManager.Query<UserPart>().List();
            var admins = userParts.Where(x => x.As<UserRolesPart>().Roles.Contains("PeergroupsAdministrator")).ToList();
            var adminEmails = admins.Select(x => x.Email).ToList();

            _mailgunClient.Send(adminEmails, "", mailSubject, html, email);
        }
    }
}