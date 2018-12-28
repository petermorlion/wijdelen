using Newtonsoft.Json;

namespace WijDelen.Contact.Models {
    public class RecaptchaResponseModel {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}