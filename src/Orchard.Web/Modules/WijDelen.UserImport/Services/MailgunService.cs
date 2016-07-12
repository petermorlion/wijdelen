using System;
using RestSharp;
using RestSharp.Authenticators;

namespace WijDelen.UserImport.Services {
    public class MailgunService : IMailService {
        public void SendUserVerificationMail(string userName) {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://api.mailgun.net/v3");
            client.Authenticator = new HttpBasicAuthenticator("api", "key-");

            var request = new RestRequest();
            request.AddParameter("domain", ".mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Mailgun Sandbox <postmaster@.mailgun.org>");
            request.AddParameter("to", "Peter Morlion <peter.morlion@gmail.com>");
            request.AddParameter("subject", "Hello Peter Morlion");
            request.AddParameter("text", "Congratulations " + userName + ", you just sent an email with Mailgun!  You are truly awesome!  You can see a record of this email in your logs: https://mailgun.com/cp/log .  You can send up to 300 emails/day from this sandbox server.  Next, you should add your own domain so you can send 10,000 emails/month for free.");
            request.Method = Method.POST;

            client.Execute(request);
        }
    }
}