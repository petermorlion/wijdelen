using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WijDelen.Mobile.Models;
using WijDelen.Mobile.Models.Jwt;

namespace WijDelen.Mobile.Providers {
    public static class JwtEncoder {
        private const string Secret = "gBWWvP23N^sWDVxL3KXget3V";

        public static string Encode(Payload payload) {
            var jsonSerializerSettings = new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var header = new Header();
            var headerPart = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(header, Formatting.None, jsonSerializerSettings));
            var payloadPart = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(payload, Formatting.None, jsonSerializerSettings));

            var secret = Secret;

            var hashImplementation = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hashImplementation.ComputeHash(Encoding.UTF8.GetBytes($"{headerPart}.{payloadPart}"));
            var hash = Base64UrlEncoder.Encode(hashBytes);

            return $"{headerPart}.{payloadPart}.{hash}";
        }

        public static bool IsValid(string jwt) {
            var authTokenParts = jwt.Split('.');

            if (authTokenParts.Length != 3) {
                return false;
            }

            var headerPart = authTokenParts[0];
            var payloadPart = authTokenParts[1];
            var hash = authTokenParts[2];

            var secret = Secret;
            var hashImplementation = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var expectedHashBytes = hashImplementation.ComputeHash(Encoding.UTF8.GetBytes($"{headerPart}.{payloadPart}"));
            var expectedHash = Base64UrlEncoder.Encode(expectedHashBytes);

            if (hash != expectedHash) {
                return false;
            }

            return true;
        }

        public static Payload DecodePayload(string jwt) {
            var authTokenParts = jwt.Split('.');

            if (authTokenParts.Length != 3) {
                throw new InvalidJwtException();
            }

            var headerPart = authTokenParts[0];
            var payloadPart = authTokenParts[1];
            var hash = authTokenParts[2];

            var secret = Secret;
            var hashImplementation = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var expectedHashBytes = hashImplementation.ComputeHash(Encoding.UTF8.GetBytes($"{headerPart}.{payloadPart}"));
            var expectedHash = Base64UrlEncoder.Encode(expectedHashBytes);

            if (hash != expectedHash) {
                throw new InvalidJwtException();
            }

            var payload = JsonConvert.DeserializeObject<Payload>(Base64UrlEncoder.Decode(payloadPart));

            return payload;
        }
    }
}