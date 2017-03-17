using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Services;
using WijDelen.UserImport.Controllers;

namespace WijDelen.UserImport.Tests.Controllers {
    [TestFixture]
    public class RegisterControllerTests {
        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT()
        {
            var controller = new RegisterController(Mock.Of<IUserService>(), Mock.Of<IMembershipService>(), Mock.Of<IUserEventHandler>());
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void TestGetWithoutNonce() {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns((IUser)null); 
            var controller = new RegisterController(userServiceMock.Object, Mock.Of<IMembershipService>(), Mock.Of<IUserEventHandler>());

            var result = controller.Index("nonce");

            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual("LogOn", ((RedirectToRouteResult)result).RouteValues["action"]);
        }

        [Test]
        public void TestGetWithNonce() {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(Mock.Of<IUser>());

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings { MinRequiredPasswordLength = 7 });

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>());

            var result = controller.Index("nonce");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual(7, ((ViewResult)result).ViewData["PasswordLength"]);
        }

        [Test]
        public void TestPostWithoutNonce()
        {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns((IUser)null);
            var controller = new RegisterController(userServiceMock.Object, Mock.Of<IMembershipService>(), Mock.Of<IUserEventHandler>());

            var result = controller.Index("nonce", "pwd", "pwd", "John", "Doe");

            Assert.IsInstanceOf<RedirectResult>(result);
            Assert.AreEqual("~/", ((RedirectResult)result).Url);
        }

        [Test]
        public void TestPostWithPasswordTooShort()
        {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(Mock.Of<IUser>());

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings { MinRequiredPasswordLength = 7 });

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>());
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "pwd", "pwd", "John", "Doe");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult)result).ViewName);
            Assert.AreEqual("You must specify a new password of 7 or more characters.", ((ViewResult)result).ViewData.ModelState["newPassword"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestPostWithPasswordsNotMatching()
        {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(Mock.Of<IUser>());

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings { MinRequiredPasswordLength = 7 });

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>());
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "password1", "password2", "John", "Doe");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult)result).ViewName);
            Assert.AreEqual("The new password and confirmation password do not match.", ((ViewResult)result).ViewData.ModelState["_FORM"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestPostWithoutFirstName()
        {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(Mock.Of<IUser>());

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings { MinRequiredPasswordLength = 7 });

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>());
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "password1", "password1", "", "Doe");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult)result).ViewName);
            Assert.AreEqual("You must specify a first name.", ((ViewResult)result).ViewData.ModelState["firstName"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestPostWithoutLastName()
        {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(Mock.Of<IUser>());

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings { MinRequiredPasswordLength = 7 });

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, Mock.Of<IUserEventHandler>());
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "password1", "password1", "John", "");

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult)result).ViewName);
            Assert.AreEqual("You must specify a last name.", ((ViewResult)result).ViewData.ModelState["lastName"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestSuccessfulPost()
        {
            var userServiceMock = new Mock<IUserService>();
            var user = Mock.Of<IUser>();
            userServiceMock.Setup(x => x.ValidateLostPassword("nonce")).Returns(user);

            var membershipServiceMock = new Mock<IMembershipService>();
            membershipServiceMock.Setup(x => x.GetSettings()).Returns(new MembershipSettings { MinRequiredPasswordLength = 7 });

            var userEventHandlerMock = new Mock<IUserEventHandler>();

            var controller = new RegisterController(userServiceMock.Object, membershipServiceMock.Object, userEventHandlerMock.Object);
            controller.T = NullLocalizer.Instance;

            var result = controller.Index("nonce", "password1", "password1", "John", "Doe");


            membershipServiceMock.Verify(x => x.SetPassword(user, "password1"));
            userEventHandlerMock.Verify(x => x.ChangedPassword(user));
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual("ChangePasswordSuccess", ((RedirectToRouteResult)result).RouteValues["action"]);
        }
    }
}