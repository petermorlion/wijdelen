using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;
using WijDelen.UserImport.Controllers;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.Tests.Mocks;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Tests.Controllers {
    [TestFixture]
    public class AdminControllerTests {
        private AdminController _controller;
        private Mock<IOrchardServices> _orchardServicesMock;
        private Mock<IAuthorizer> _authorizerMock;
        private Mock<IMembershipService> _membershipServiceMock;
        private IContainer _container;
        private Mock<IMailService> _mailServiceMock;
        private Mock<IUserImportService> _userImportServiceMock;
        private Mock<ICsvReader> _csvReaderMock;
        private Mock<IGroupService> _groupServiceMock;
        private Mock<INotifier> _notifierMock;

        [SetUp]
        public void Init()
        {
            var builder = new ContainerBuilder();

            _orchardServicesMock = new Mock<IOrchardServices>();
            _authorizerMock = new Mock<IAuthorizer>();
            _membershipServiceMock = new Mock<IMembershipService>();
            _csvReaderMock = new Mock<ICsvReader>();
            _userImportServiceMock = new Mock<IUserImportService>();
            _mailServiceMock = new Mock<IMailService>();
            _groupServiceMock = new Mock<IGroupService>();
            _notifierMock = new Mock<INotifier>();

            builder.RegisterInstance(_orchardServicesMock.Object).As<IOrchardServices>();
            builder.RegisterInstance(_authorizerMock.Object).As<IAuthorizer>();
            builder.RegisterInstance(_membershipServiceMock.Object).As<IMembershipService>();
            builder.RegisterInstance(_mailServiceMock.Object).As<IMailService>();
            builder.RegisterInstance(_userImportServiceMock.Object).As<IUserImportService>();
            builder.RegisterInstance(_csvReaderMock.Object).As<ICsvReader>();
            builder.RegisterInstance(_groupServiceMock.Object).As<IGroupService>();
            builder.RegisterInstance(_notifierMock.Object).As<INotifier>();
            builder.RegisterType<AdminController>();

            _container = builder.Build();
            _controller = _container.Resolve<AdminController>();
        }

        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var localizer = NullLocalizer.Instance;

            _controller.T = localizer;

            Assert.AreEqual(localizer, _controller.T);
        }

        [Test]
        public void TestIndexWithoutAuthorization() {
            _orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            _authorizerMock.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(false);

            var result = _controller.Index();

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }

        [Test]
        public void TestIndexWithAuthorization() {
            _orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            _authorizerMock.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(true);

            var result = _controller.Index();

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsInstanceOf<AdminIndexViewModel>(((ViewResult)result).Model);
            Assert.AreEqual("", ((ViewResult)result).ViewName);
        }

        [Test]
        public void TestIndexPostWithoutAuthorization() {
            _orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            _authorizerMock.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(false);

            var result = _controller.Index(null);

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }

        [Test]
        public void TestIndexPostWithAuthorization() {
            using (var memoryStream = new MemoryStream()) {
                _orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
                _authorizerMock.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(true);

                var site = new Mock<ISite>();
                var mockWorkContext = new MockWorkContext {CurrentSite = site.Object};
                _orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
                site.Setup(x => x.BaseUrl).Returns("baseUrl");

                var users = new List<User>();
                _csvReaderMock.Setup(x => x.ReadUsers(memoryStream)).Returns(users);

                var userFactory = new UserMockFactory();
                var john = userFactory.Create("john", "john.doe@example.com", "John", "Doe");
                
                var userImportResults = new List<UserImportResult> {
                    new UserImportResult("john", "john.doe@example.com") { User = john },
                    new UserImportResult("jane", "jane.doe@example.com")
                };
                _userImportServiceMock.Setup(x => x.ImportUsers(users)).Returns(userImportResults);

                IList<IUser> importedUsers = null;
                _mailServiceMock
                    .Setup(x => x.SendUserVerificationMails(It.IsAny<IEnumerable<IUser>>(), It.IsAny<Func<string, string>>()))
                    .Callback((IEnumerable<IUser> r, Func<string, string> f) => importedUsers = r.ToList());

                var usersFile = new Mock<HttpPostedFileBase>();
                usersFile.Setup(x => x.InputStream).Returns(memoryStream);

                var viewModel = new AdminIndexViewModel();
                viewModel.File = usersFile.Object;
                viewModel.NewGroupName = "New Group";

                var result = _controller.Index(viewModel);

                Assert.IsInstanceOf<ViewResult>(result);
                Assert.IsInstanceOf<IList<UserImportResult>>(((ViewResult)result).Model);
                Assert.AreEqual("ImportComplete", ((ViewResult)result).ViewName);
                Assert.AreEqual(1, importedUsers.Count);
                Assert.AreEqual("john", importedUsers.Single().UserName);
                Assert.AreEqual("john.doe@example.com", importedUsers.Single().Email);

                _groupServiceMock.Verify(x => x.AddUsersToGroup("New Group", It.IsAny<IEnumerable<IUser>>()));
            }
        }

        [Test]
        public void TestIndexPostWithoutNewGroupName() {
            _orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            _authorizerMock.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(true);

            var viewModel = new AdminIndexViewModel {
                NewGroupName = "",
                UserImportLinkMode = UserImportLinkMode.New
            };

            var result = _controller.Index(viewModel);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult)result).ViewName);
            Assert.AreEqual("Please provide a group name to create a new group.", ((ViewResult)result).ViewData.ModelState["NewGroupName"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestResendUserVerificationMail() {
            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(2);

            var site = new Mock<ISite>();
            var mockWorkContext = new MockWorkContext { CurrentSite = site.Object };
            _orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            site.Setup(x => x.BaseUrl).Returns("baseUrl");
            _membershipServiceMock.Setup(x => x.GetUser("moe")).Returns(userMock.Object);

            IList<IUser> importedUsers = null;
            _mailServiceMock
                .Setup(x => x.SendUserVerificationMails(It.IsAny<IEnumerable<IUser>>(), It.IsAny<Func<string, string>>()))
                .Callback((IEnumerable<IUser> r, Func<string, string> f) => importedUsers = r.ToList());

            var result = _controller.ResendUserVerificationMail("moe");

            Assert.AreEqual(userMock.Object, importedUsers.Single());
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual("Edit", ((RedirectToRouteResult)result).RouteValues["action"]);
            Assert.AreEqual("Orchard.Users", ((RedirectToRouteResult)result).RouteValues["area"]);
            Assert.AreEqual("Admin", ((RedirectToRouteResult)result).RouteValues["controller"]);
            Assert.AreEqual(2, ((RedirectToRouteResult)result).RouteValues["id"]);

            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("User verification mail has been sent.")));
        }
    }
}