using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using WijDelen.UserImport.Controllers;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.Tests.Mocks;

namespace WijDelen.UserImport.Tests.Controllers {
    [TestFixture]
    public class AdminControllerTests {
        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var controller = new AdminController(Mock.Of<IOrchardServices>(), Mock.Of<ICsvReader>(), Mock.Of<IUserImportService>(), Mock.Of<IMailService>());
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void TestIndexWithoutAuthorization() {
            var orchardServices = new Mock<IOrchardServices>();
            var authorizer = new Mock<IAuthorizer>();
            orchardServices.Setup(x => x.Authorizer).Returns(authorizer.Object);
            authorizer.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(false);
            var controller = new AdminController(orchardServices.Object, Mock.Of<ICsvReader>(), Mock.Of<IUserImportService>(), Mock.Of<IMailService>());

            var result = controller.Index();

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }

        [Test]
        public void TestIndexWithAuthorization() {
            var orchardServices = new Mock<IOrchardServices>();
            var authorizer = new Mock<IAuthorizer>();
            orchardServices.Setup(x => x.Authorizer).Returns(authorizer.Object);
            authorizer.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(true);
            var controller = new AdminController(orchardServices.Object, Mock.Of<ICsvReader>(), Mock.Of<IUserImportService>(), Mock.Of<IMailService>());

            var result = controller.Index();

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsInstanceOf<AdminIndexViewModel>(((ViewResult)result).Model);
            Assert.AreEqual("", ((ViewResult)result).ViewName);
        }

        [Test]
        public void TestIndexPostWithoutAuthorization() {
            var orchardServices = new Mock<IOrchardServices>();
            var authorizer = new Mock<IAuthorizer>();
            orchardServices.Setup(x => x.Authorizer).Returns(authorizer.Object);
            authorizer.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(false);
            var controller = new AdminController(orchardServices.Object, Mock.Of<ICsvReader>(), Mock.Of<IUserImportService>(), Mock.Of<IMailService>());

            var result = controller.Index(null, "", "");

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }

        [Test]
        public void TestIndexPostWithAuthorization() {
            using (var memoryStream = new MemoryStream()) {
                var orchardServices = new Mock<IOrchardServices>();
                var authorizer = new Mock<IAuthorizer>();

                orchardServices.Setup(x => x.Authorizer).Returns(authorizer.Object);
                authorizer.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(true);

                var site = new Mock<ISite>();
                var mockWorkContext = new MockWorkContext {CurrentSite = site.Object};
                orchardServices.Setup(x => x.WorkContext).Returns(mockWorkContext);
                site.Setup(x => x.BaseUrl).Returns("baseUrl");

                var users = new List<User>();
                var csvReader = new Mock<ICsvReader>();
                csvReader.Setup(x => x.ReadUsers(memoryStream)).Returns(users);

                var userImportResults = new List<UserImportResult> {
                    new UserImportResult("john", "john.doe@example.com") { User = Mock.Of<IUser>() },
                    new UserImportResult("jane", "jane.doe@example.com")
                };
                var userImportService = new Mock<IUserImportService>();
                userImportService.Setup(x => x.ImportUsers(users)).Returns(userImportResults);

                IList<UserImportResult> importedUsers = null;
                var mailService = new Mock<IMailService>();
                mailService
                    .Setup(x => x.SendUserVerificationMails(It.IsAny<IEnumerable<UserImportResult>>(), It.IsAny<Func<string, string>>()))
                    .Callback((IEnumerable<UserImportResult> r, Func<string, string> f) => importedUsers = r.ToList());
                
                var controller = new AdminController(orchardServices.Object, csvReader.Object, userImportService.Object, mailService.Object);
                var usersFile = new Mock<HttpPostedFileBase>();
                usersFile.Setup(x => x.InputStream).Returns(memoryStream);

                var result = controller.Index(usersFile.Object, "", "");

                Assert.IsInstanceOf<ViewResult>(result);
                Assert.IsInstanceOf<IList<UserImportResult>>(((ViewResult)result).Model);
                Assert.AreEqual("ImportComplete", ((ViewResult)result).ViewName);
                Assert.AreEqual(1, importedUsers.Count);
                Assert.AreEqual("john", importedUsers.Single().UserName);
                Assert.AreEqual("john.doe@example.com", importedUsers.Single().Email);
            }
        }
    }
}