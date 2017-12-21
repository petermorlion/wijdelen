using FluentAssertions;
using NUnit.Framework;
using WijDelen.Mobile.Models.Jwt;
using WijDelen.Mobile.Providers;

namespace WijDelen.Mobile.Tests.Providers {
    [TestFixture]
    public class JwtEncoderTest {
        [Test]
        public void Encode_ShouldReturnJwt() {
            var payload = new Payload {
                UserId = 1,
                UserEmail = "john.doe@example.com",
                UserName = "john.doe"
            };

            var jwt = JwtEncoder.Encode(payload);

            jwt.Should().Be("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOjEsInVzZXJFbWFpbCI6ImpvaG4uZG9lQGV4YW1wbGUuY29tIiwidXNlck5hbWUiOiJqb2huLmRvZSJ9.Wg2Qkhe4SaIpy9eOI5nJEZOhNt_uaAUlP_Gw-C0PYhA");
        }
    }
}