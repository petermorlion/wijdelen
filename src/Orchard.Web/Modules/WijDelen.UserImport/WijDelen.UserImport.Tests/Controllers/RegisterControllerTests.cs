using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Services;
using WijDelen.UserImport.Controllers;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.Tests.Mocks;

namespace WijDelen.UserImport.Tests.Controllers {
    [TestFixture]
    public class RegisterControllerTests {
        [Test]
        public void TestGetWithNonce() {
            var userMockFactory = new UserMockFactory();
            var userMock = userMockFactory.Create("moe@simpsons.com", "moe@simpsons.com", "", "", "nl-BE");

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(userMock);

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings {MinRequiredPasswordLength = 7});

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>(), Mock.Of<IUpdateUserDetailsService>());

            var result = controller.Index("nonce");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual(7, ((ViewResult) result).ViewData["PasswordLength"]);
        }

        [Test]
        public void TestGetWithNonceAndUserAlreadyHasFirstAndLastName() {
            var userMockFactory = new UserMockFactory();
            var userMock = userMockFactory.Create("moe@simpsons.com", "moe@simpsons.com", "Moe", "Szyslak", "nl-BE");

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(userMock);

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings {MinRequiredPasswordLength = 7});

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>(), Mock.Of<IUpdateUserDetailsService>());

            var result = controller.Index("nonce");

            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual("LogOn", ((RedirectToRouteResult) result).RouteValues["action"]);
            Assert.AreEqual("Account", ((RedirectToRouteResult) result).RouteValues["controller"]);
            Assert.AreEqual("Orchard.Users", ((RedirectToRouteResult) result).RouteValues["area"]);
        }

        [Test]
        public void TestGetWithoutNonce() {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns((IUser) null);
            var controller = new RegisterController(userServiceMock.Object, Mock.Of<IMembershipService>(), Mock.Of<IUserEventHandler>(), Mock.Of<IUpdateUserDetailsService>());

            var result = controller.Index("nonce");

            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual("LogOn", ((RedirectToRouteResult) result).RouteValues["action"]);
            Assert.AreEqual("Orchard.Users", ((RedirectToRouteResult) result).RouteValues["area"]);
            Assert.AreEqual("Account", ((RedirectToRouteResult) result).RouteValues["controller"]);
        }

        [Test]
        public void TestPostWithoutCulture() {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(Mock.Of<IUser>());

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings {MinRequiredPasswordLength = 7});

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>(), Mock.Of<IUpdateUserDetailsService>());
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "password1", "password1", "John", "Doe", "");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult) result).ViewName);
            Assert.AreEqual("You must specify a language.", ((ViewResult) result).ViewData.ModelState["culture"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestPostWithoutFirstName() {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(Mock.Of<IUser>());

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings {MinRequiredPasswordLength = 7});

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>(), Mock.Of<IUpdateUserDetailsService>());
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "password1", "password1", "", "Doe", "nl-BE");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult) result).ViewName);
            Assert.AreEqual("You must specify a first name.", ((ViewResult) result).ViewData.ModelState["firstName"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestPostWithoutLastName() {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(Mock.Of<IUser>());

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings {MinRequiredPasswordLength = 7});

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>(), Mock.Of<IUpdateUserDetailsService>());
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "password1", "password1", "John", "", "nl-BE");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult) result).ViewName);
            Assert.AreEqual("You must specify a last name.", ((ViewResult) result).ViewData.ModelState["lastName"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestPostWithoutNonce() {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns((IUser) null);
            var controller = new RegisterController(userServiceMock.Object, Mock.Of<IMembershipService>(), Mock.Of<IUserEventHandler>(), Mock.Of<IUpdateUserDetailsService>());

            var result = controller.Index("nonce", "pwd", "pwd", "John", "Doe", "nl-BE");

            Assert.IsInstanceOf<RedirectResult>(result);
            Assert.AreEqual("~/", ((RedirectResult) result).Url);
        }

        [Test]
        public void TestPostWithPasswordsNotMatching() {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(Mock.Of<IUser>());

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings {MinRequiredPasswordLength = 7});

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>(), Mock.Of<IUpdateUserDetailsService>());
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "password1", "password2", "John", "Doe", "nl-BE");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult) result).ViewName);
            Assert.AreEqual("The new password and confirmation password do not match.", ((ViewResult) result).ViewData.ModelState["_FORM"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestPostWithPasswordTooShort() {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(Mock.Of<IUser>());

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings {MinRequiredPasswordLength = 7});

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>(), Mock.Of<IUpdateUserDetailsService>());
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "pwd", "pwd", "John", "Doe", "nl-BE");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult) result).ViewName);
            Assert.AreEqual("You must specify a new password of 7 or more characters.", ((ViewResult) result).ViewData.ModelState["newPassword"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestSuccessfulPost() {
            var user = new Mock<IUser>();

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(user.Object);

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings {MinRequiredPasswordLength = 7});

            var userEventHandlerMock = new Mock<IUserEventHandler>();

            var updateUserDetailsServiceMock = new Mock<IUpdateUserDetailsService>();

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, userEventHandlerMock.Object, updateUserDetailsServiceMock.Object);
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "password1", "password1", "John", "Doe", "nl-BE");

            updateUserDetailsServiceMock.Verify(x => x.UpdateUserDetails(user.Object, "John", "Doe", "nl-BE", true));
            membershipServiceMock.Verify(x => x.SetPassword(user.Object, "password1"));
            userEventHandlerMock.Verify(x => x.ChangedPassword(user.Object));
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual("Index", ((RedirectToRouteResult) result).RouteValues["action"]);
            Assert.AreEqual("WijDelen.ObjectSharing", ((RedirectToRouteResult) result).RouteValues["area"]);
            Assert.AreEqual("GetStarted", ((RedirectToRouteResult) result).RouteValues["controller"]);
        }

        /// <summary>
        ///     Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a
        ///     runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var controller = new RegisterController(Mock.Of<IUserService>(), Mock.Of<IMembershipService>(), Mock.Of<IUserEventHandler>(), Mock.Of<IUpdateUserDetailsService>());
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }
    }
}