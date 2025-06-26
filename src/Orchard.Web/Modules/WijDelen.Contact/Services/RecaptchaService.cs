using System.Globalization;
using System.IO;
using System.Net;
using Orchard;
using Orchard.Services;
using WijDelen.Contact.Models;

namespace WijDelen.Contact.Services {
    public class RecaptchaService : IRecaptchaService {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IJsonConverter _jsonConverter;

        public RecaptchaService(IWorkContextAccessor workContextAccessor, IJsonConverter jsonConverter) {
            _workContextAccessor = workContextAccessor;
            _jsonConverter = jsonConverter;
        }

        public bool Validates() {
            var context = _workContextAccessor.GetContext().HttpContext;
            var recaptchaResponse = context.Request.Form["g-recaptcha-response"];

            var privateKey = "...";
            var remoteIp = context.Request.ServerVariables["REMOTE_ADDR"];

            var postData = string.Format(CultureInfo.InvariantCulture, "secret={0}&response={1}&remoteip={2}", privateKey, recaptchaResponse, remoteIp);
            var fullUri = "https://www.google.com/recaptcha/api/siteverify" + "?" + postData;

            var request = WebRequest.Create(fullUri);
            request.Method = "GET";
            request.Timeout = 5000; //milliseconds
            request.ContentType = "application/x-www-form-urlencoded";

            string json;

            using (var webResponse = request.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }
            }

            var responseModel = _jsonConverter.Deserialize<RecaptchaResponseModel>(json);

            return responseModel.Success;
        }
    }
}
