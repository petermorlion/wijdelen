using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Orchard.Logging;
using Orchard.Services;
using RestSharp;
using RestSharp.Authenticators;

namespace WijDelen.MailChimp {
    public class MailChimpClient : IMailChimpClient {
        private readonly IJsonConverter _jsonConverter;
        private readonly string _apiKey;
        private readonly Uri _apiBaseUrl;
        private readonly string _listId;

        public MailChimpClient(IJsonConverter jsonConverter) {
            _jsonConverter = jsonConverter;

            Logger = NullLogger.Instance;

            _apiKey = "43a96c957b782a875beca1b56d9c854a-us16";
            _apiBaseUrl = new Uri("https://us16.api.mailchimp.com/3.0/");

#if DEBUG
            _listId = "59f3bbf435";
#else
            _listId = "1a8b1308c7"; 
#endif
        }

        public ILogger Logger { get; set; }

        public bool IsSubscribed(string email) {
            var client = new RestClient {
                BaseUrl = _apiBaseUrl,
                Authenticator = new HttpBasicAuthenticator("api", _apiKey)
            };

            var emailHash = GetHash(email);

            var request = new RestRequest();
            request.Resource = "lists/{listId}/members/{emailHash}";
            request.AddUrlSegment("listId", _listId);
            request.AddUrlSegment("emailHash", emailHash);
            request.Method = Method.GET;
            var response = client.Execute(request);

            return response.StatusCode == HttpStatusCode.OK && _jsonConverter.Deserialize<dynamic>(response.Content).status == "subscribed";
        }

        public void Subscribe(string email) {
            ChangeSubscriptionStatus(email, "subscribed");
        }

        public void Unsubscribe(string email) {
            ChangeSubscriptionStatus(email, "unsubscribed");
        }

        private void ChangeSubscriptionStatus(string email, string status) {
            var client = new RestClient
            {
                BaseUrl = _apiBaseUrl,
                Authenticator = new HttpBasicAuthenticator("api", _apiKey)
            };

            var emailHash = GetHash(email);

            var request = new RestRequest();
            request.Resource = "lists/{listId}/members/{emailHash}";
            request.AddUrlSegment("listId", _listId);
            request.AddUrlSegment("emailHash", emailHash);
            request.AddJsonBody(new { email_address = email, status = status, statues_if_new = status, merge_fields = new { FNAME = "", LNAME = "", } });
            request.Method = Method.PUT;
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK) {
                Logger.Warning("Something happened");
            }
        }

        private static string GetHash(string input) {
            using (var md5Hash = MD5.Create()) {
                var hashBytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                var stringBuilder = new StringBuilder();
                foreach (var hashByte in hashBytes)
                    stringBuilder.Append(hashByte.ToString("x2"));

                return stringBuilder.ToString();
            }
        }
    }
}