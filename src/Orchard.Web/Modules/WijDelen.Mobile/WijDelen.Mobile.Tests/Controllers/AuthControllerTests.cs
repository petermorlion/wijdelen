using System.Net;
using System.Net.Http;
using System.Web.Http;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Security;
using Orchard.Users.Events;
using WijDelen.Mobile.Controllers;
using WijDelen.Mobile.Models;

namespace WijDelen.Mobile.Tests.Controllers {
    [TestFixture]
    public class AuthControllerTests {
        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            _userEventHandlerMock = new Mock<IUserEventHandler>();

            _userMock = new Mock<IUser>();
            _userMock.Setup(x => x.Id).Returns(1);
            _userMock.Setup(x => x.Email).Returns(_validEmail);
            _userMock.Setup(x => x.UserName).Returns(_validEmail);

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock
                .Setup(x => x.ValidateUser(_validEmail, _validPassword))
                .Returns(_userMock.Object);

            builder.RegisterInstance(membershipServiceMock.Object).As<IMembershipService>();
            builder.RegisterInstance(_userEventHandlerMock.Object).As<IUserEventHandler>();
            builder.RegisterType<AuthController>();

            var container = builder.Build();
            _controller = container.Resolve<AuthController>();

            _controller.Configuration = new HttpConfiguration();
            _controller.Request = new HttpRequestMessage();
        }

        private AuthController _controller;
        private Mock<IUserEventHandler> _userEventHandlerMock;
        private readonly string _validEmail = "john.doe@example.com";
        private readonly string _validPassword = "abcdef";
        private Mock<IUser> _userMock;

        [Test]
        public void Post_WithInvalidCredentials_ShouldReturnUnauthorized() {
            var loginModel = new LoginModel {
                UserNameOrEmail = "foo",
                Password = "bar"
            };

            var response = _controller.Post(loginModel);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            _userEventHandlerMock.Verify(x => x.LoggingIn("foo", "bar"));
            _userEventHandlerMock.Verify(x => x.LogInFailed("foo", "bar"));
        }

        [Test]
        public void Post_WithValidCredentials_ShouldReturnJwt() {
            var loginModel = new LoginModel {
                UserNameOrEmail = _validEmail,
                Password = _validPassword
            };

            var response = _controller.Post(loginModel);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.ReadAsStringAsync().Result.Should().Be("\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOjEsInVzZXJFbWFpbCI6ImpvaG4uZG9lQGV4YW1wbGUuY29tIiwidXNlck5hbWUiOiJqb2huLmRvZUBleGFtcGxlLmNvbSJ9.1wCBEksg0jAFAnMjhzWtHYyXMzO-kD8YFmKEi5TXdFI\"");

            _userEventHandlerMock.Verify(x => x.LoggingIn(_validEmail, _validPassword));
            _userEventHandlerMock.Verify(x => x.LoggedIn(_userMock.Object));
        }
    }
}