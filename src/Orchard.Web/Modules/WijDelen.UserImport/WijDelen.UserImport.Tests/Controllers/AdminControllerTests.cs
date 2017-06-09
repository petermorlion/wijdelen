using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
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
        private Mock<IGroupService> _groupServiceMock;
        private Mock<INotifier> _notifierMock;

        [SetUp]
        public void Init()
        {
            var builder = new ContainerBuilder();

            _orchardServicesMock = new Mock<IOrchardServices>();
            _authorizerMock = new Mock<IAuthorizer>();
            _membershipServiceMock = new Mock<IMembershipService>();
            _userImportServiceMock = new Mock<IUserImportService>();
            _mailServiceMock = new Mock<IMailService>();
            _groupServiceMock = new Mock<IGroupService>();
            _notifierMock = new Mock<INotifier>();

            builder.RegisterInstance(_orchardServicesMock.Object).As<IOrchardServices>();
            builder.RegisterInstance(_authorizerMock.Object).As<IAuthorizer>();
            builder.RegisterInstance(_membershipServiceMock.Object).As<IMembershipService>();
            builder.RegisterInstance(_mailServiceMock.Object).As<IMailService>();
            builder.RegisterInstance(_userImportServiceMock.Object).As<IUserImportService>();
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
            _orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            _authorizerMock.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(true);

            var site = new Mock<ISite>();
            var mockWorkContext = new MockWorkContext { CurrentSite = site.Object };
            _orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            site.Setup(x => x.BaseUrl).Returns("baseUrl");

            var userFactory = new UserMockFactory();
            var john = userFactory.Create("john", "john.doe@example.com", "John", "Doe", "nl-BE", GroupMembershipStatus.Pending);

            var userImportResults = new List<UserImportResult> {
                    new UserImportResult("john.doe@example.com") { User = john },
                    new UserImportResult("jane.doe@example.com")
                };

            IList<string> importedEmails = null;
            _userImportServiceMock
                .Setup(x => x.ImportUsers(It.IsAny<IList<string>>()))
                .Callback((IList<string> e) => importedEmails = e)
                .Returns(userImportResults);

            IList<IUser> importedUsers = null;
            _mailServiceMock
                .Setup(x => x.SendUserInvitationMails(It.IsAny<IEnumerable<IUser>>(), It.IsAny<Func<string, string>>(), "The Group", ""))
                .Callback((IEnumerable<IUser> r, Func<string, string> f, string gn, string gu) => importedUsers = r.ToList());

            var viewModel = new AdminIndexViewModel
            {
                UserEmails = "john.doe@example.com" + Environment.NewLine + "jane.doe@example.com",
                SelectedGroupId = 123456
            };

            _groupServiceMock.Setup(x => x.GetGroups()).Returns(new[] { new GroupViewModel { Id = 123456, Name = "The Group", LogoUrl = "" } });

            var result = _controller.Index(viewModel);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsInstanceOf<IList<UserImportResult>>(((ViewResult)result).Model);
            Assert.AreEqual("ImportComplete", ((ViewResult)result).ViewName);
            Assert.AreEqual(1, importedUsers.Count);
            Assert.AreEqual("john", importedUsers.Single().UserName);
            Assert.AreEqual("john.doe@example.com", importedUsers.Single().Email);
            importedEmails.ShouldBeEquivalentTo(new[] { "jane.doe@example.com", "john.doe@example.com" });

            _groupServiceMock.Verify(x => x.AddUsersToGroup("The Group", It.IsAny<IEnumerable<IUser>>()));
        }

        [Test]
        public void TestResendUserInvitationMail() {
            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(2);

            var site = new Mock<ISite>();
            var mockWorkContext = new MockWorkContext { CurrentSite = site.Object };
            _orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            site.Setup(x => x.BaseUrl).Returns("baseUrl");
            _membershipServiceMock.Setup(x => x.GetUser("moe")).Returns(userMock.Object);

            var groupViewModel = new GroupViewModel { Name = "Existing group", LogoUrl = "url" };
            _groupServiceMock.Setup(x => x.GetGroupForUser(2)).Returns(groupViewModel);

            IList<IUser> importedUsers = null;
            _mailServiceMock
                .Setup(x => x.SendUserInvitationMails(It.IsAny<IEnumerable<IUser>>(), It.IsAny<Func<string, string>>(), "Existing group", "url"))
                .Callback((IEnumerable<IUser> r, Func<string, string> f, string gn, string gu) => importedUsers = r.ToList());

            var result = _controller.ResendUserInvitationMail("moe");

            Assert.AreEqual(userMock.Object, importedUsers.Single());
            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual("Edit", ((RedirectToRouteResult)result).RouteValues["action"]);
            Assert.AreEqual("Orchard.Users", ((RedirectToRouteResult)result).RouteValues["area"]);
            Assert.AreEqual("Admin", ((RedirectToRouteResult)result).RouteValues["controller"]);
            Assert.AreEqual(2, ((RedirectToRouteResult)result).RouteValues["id"]);

            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("User invitation mail has been sent.")));
        }

        [Test]
        public void TestResendUserInvitationMailWithoutGroup() {
            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(2);

            var site = new Mock<ISite>();
            var mockWorkContext = new MockWorkContext { CurrentSite = site.Object };
            _orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            site.Setup(x => x.BaseUrl).Returns("baseUrl");
            _membershipServiceMock.Setup(x => x.GetUser("moe")).Returns(userMock.Object);

            _groupServiceMock.Setup(x => x.GetGroupForUser(2)).Returns((GroupViewModel) null);

            var result = _controller.ResendUserInvitationMail("moe");

            Assert.IsInstanceOf<RedirectToRouteResult>(result);
            Assert.AreEqual("Edit", ((RedirectToRouteResult)result).RouteValues["action"]);
            Assert.AreEqual("Orchard.Users", ((RedirectToRouteResult)result).RouteValues["area"]);
            Assert.AreEqual("Admin", ((RedirectToRouteResult)result).RouteValues["controller"]);
            Assert.AreEqual(2, ((RedirectToRouteResult)result).RouteValues["id"]);

            _notifierMock.Verify(x => x.Add(NotifyType.Warning, new LocalizedString("The user needs to be part of a group first.")));

            _mailServiceMock
                .Verify(x => x.SendUserInvitationMails(
                    It.IsAny<IEnumerable<IUser>>(),
                    It.IsAny<Func<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()), Times.Never);
        }
    }
}