using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Security;
using WijDelen.UserImport.Controllers;

namespace WijDelen.UserImport.Tests.Controllers {
    [TestFixture]
    public class AdminControllerTests {
        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var controller = new AdminController(Mock.Of<IAuthorizer>());
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void TestIndexWithoutAuthorization() {
            var authorizer = new Mock<IAuthorizer>();
            authorizer.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(false);
            var controller = new AdminController(authorizer.Object);

            var result = controller.Index();

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }
    }
}