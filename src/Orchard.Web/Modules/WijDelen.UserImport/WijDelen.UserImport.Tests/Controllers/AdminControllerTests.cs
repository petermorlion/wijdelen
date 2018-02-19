using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
using Orchard.Localization.Services;
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
        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            _orchardServicesMock = new Mock<IOrchardServices>();
            _authorizerMock = new Mock<IAuthorizer>();
            _membershipServiceMock = new Mock<IMembershipService>();
            _userImportServiceMock = new Mock<IUserImportService>();
            _mailServiceMock = new Mock<IMailService>();
            _groupServiceMock = new Mock<IGroupService>();
            _notifierMock = new Mock<INotifier>();
            _cultureManagerMock = new Mock<ICultureManager>();

            builder.RegisterInstance(_orchardServicesMock.Object).As<IOrchardServices>();
            builder.RegisterInstance(_authorizerMock.Object).As<IAuthorizer>();
            builder.RegisterInstance(_membershipServiceMock.Object).As<IMembershipService>();
            builder.RegisterInstance(_mailServiceMock.Object).As<IMailService>();
            builder.RegisterInstance(_userImportServiceMock.Object).As<IUserImportService>();
            builder.RegisterInstance(_groupServiceMock.Object).As<IGroupService>();
            builder.RegisterInstance(_notifierMock.Object).As<INotifier>();
            builder.RegisterInstance(_cultureManagerMock.Object).As<ICultureManager>();
            builder.RegisterType<AdminController>();

            _container = builder.Build();
            _controller = _container.Resolve<AdminController>();
        }

        private AdminController _controller;
        private Mock<IOrchardServices> _orchardServicesMock;
        private Mock<IAuthorizer> _authorizerMock;
        private Mock<IMembershipService> _membershipServiceMock;
        private IContainer _container;
        private Mock<IMailService> _mailServiceMock;
        private Mock<IUserImportService> _userImportServiceMock;
        private Mock<IGroupService> _groupServiceMock;
        private Mock<INotifier> _notifierMock;
        private Mock<ICultureManager> _cultureManagerMock;

        [Test]
        public void TestIndexPostWithAuthorization() {
            _orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            _authorizerMock.Setup(x => x.Authorize(Permissions.ImportUsers, It.IsAny<LocalizedString>())).Returns(true);

            var site = new Mock<ISite>();
            var mockWorkContext = new MockWorkContext {CurrentSite = site.Object};
            _orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            site.Setup(x => x.BaseUrl).Returns("baseUrl");

            var userFactory = new UserMockFactory();
            var john = userFactory.Create("john", "john.doe@example.com", "John", "Doe", "nl-BE", GroupMembershipStatus.Pending);

            var userImportResults = new List<UserImportResult> {
                new UserImportResult("john.doe@example.com") {User = john},
                new UserImportResult("jane.doe@example.com")
            };

            IList<string> importedEmails = null;
            _userImportServiceMock
                .Setup(x => x.ImportUsers("test", It.IsAny<IList<string>>()))
                .Callback((string culture, IList<string> e) => importedEmails = e)
                .Returns(userImportResults);

            IList<IUser> importedUsers = null;
            _mailServiceMock
                .Setup(x => x.SendUserInvitationMails("test", It.IsAny<IEnumerable<IUser>>(), It.IsAny<Func<string, string>>(), "The Group", "", "<p>Extra info</p>"))
                .Callback((string culture, IEnumerable<IUser> r, Func<string, string> f, string gn, string gu, string ei) => importedUsers = r.ToList());

            var viewModel = new AdminIndexViewModel {
                UserEmails = "john.doe@example.com" + Environment.NewLine + "jane.doe@example.com",
                SelectedGroupId = 123456,
                CultureForMails = "test",
                ExtraInfoHtml = "<p>Extra info</p>"
            };

            _groupServiceMock.Setup(x => x.GetGroups()).Returns(new[] {new GroupViewModel {Id = 123456, Name = "The Group", LogoUrl = ""}});

            var result = _controller.Index(viewModel);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsInstanceOf<IList<UserImportResult>>(((ViewResult) result).Model);
            Assert.AreEqual("ImportComplete", ((ViewResult) result).ViewName);
            Assert.AreEqual(1, importedUsers.Count);
            Assert.AreEqual("john", importedUsers.Single().UserName);
            Assert.AreEqual("john.doe@example.com", importedUsers.Single().Email);
            importedEmails.ShouldBeEquivalentTo(new[] {"jane.doe@example.com", "john.doe@example.com"});

            _groupServiceMock.Verify(x => x.AddUsersToGroup("The Group", It.IsAny<IEnumerable<IUser>>()));
        }

        [Test]
        public void TestIndexPostWithoutAuthorization() {
            _orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            _authorizerMock.Setup(x => x.Authorize(Permissions.ImportUsers, It.IsAny<LocalizedString>())).Returns(false);

            var result = _controller.Index(null);

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }

        [Test]
        public void TestIndexWithAuthorization() {
            _orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            _authorizerMock.Setup(x => x.Authorize(Permissions.ImportUsers, It.IsAny<LocalizedString>())).Returns(true);
            var cultures = new List<string> {"en", "nl", "test"};
            _cultureManagerMock.Setup(x => x.ListCultures()).Returns(cultures);
            _cultureManagerMock.Setup(x => x.GetSiteCulture()).Returns("test");
            var groupViewModels = new[] {new GroupViewModel {Id = 123456, Name = "The Group", LogoUrl = ""}};
            _groupServiceMock.Setup(x => x.GetGroups()).Returns(groupViewModels);

            var result = _controller.Index();

            Assert.IsInstanceOf<ViewResult>(result);

            var viewResult = (ViewResult) result;
            Assert.IsInstanceOf<AdminIndexViewModel>(viewResult.Model);

            viewResult.ViewName.Should().Be("");

            var viewModel = (AdminIndexViewModel) viewResult.Model;
            viewModel.Groups.ShouldBeEquivalentTo(groupViewModels);
            viewModel.SiteCultures.ShouldBeEquivalentTo(cultures);
            viewModel.CultureForMails.ShouldBeEquivalentTo("test");
        }

        [Test]
        public void TestIndexWithoutAuthorization() {
            _orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            _authorizerMock.Setup(x => x.Authorize(Permissions.ImportUsers, It.IsAny<LocalizedString>())).Returns(false);

            var result = _controller.Index();

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }

        /// <summary>
        ///     Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a
        ///     runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var localizer = NullLocalizer.Instance;

            _controller.T = localizer;

            Assert.AreEqual(localizer, _controller.T);
        }
    }
}