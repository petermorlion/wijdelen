using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using WijDelen.UserImport.Controllers;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.Tests.Mocks;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Tests.Controllers {
    [TestFixture]
    public class GroupUsersControllerTests {
        private GroupUsersController _controller;
        private Mock<IAuthorizer> _authorizerMock;
        private Mock<IGroupService> _groupServiceMock;
        private Mock<IMailService> _mailServiceMock;
        private Mock<INotifier> _notifierMock;

        [SetUp]
        public void Init() {
            var site = new Mock<ISite>();
            site.Setup(x => x.BaseUrl).Returns("baseUrl");
            var mockWorkContext = new MockWorkContext { CurrentSite = site.Object };

            _authorizerMock = new Mock<IAuthorizer>();
            _notifierMock = new Mock<INotifier>();

            var orchardServicesMock = new Mock<IOrchardServices>();
            orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            orchardServicesMock.Setup(x => x.Notifier).Returns(_notifierMock.Object);
            
            var siteServiceMock = new Mock<ISiteService>();
            var shapeFactoryMock = new Mock<IShapeFactory>();
            _groupServiceMock = new Mock<IGroupService>();
            _mailServiceMock = new Mock<IMailService>();
            
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<GroupUsersController>();
            containerBuilder.RegisterInstance(orchardServicesMock.Object).As<IOrchardServices>();
            containerBuilder.RegisterInstance(siteServiceMock.Object).As<ISiteService>();
            containerBuilder.RegisterInstance(shapeFactoryMock.Object).As<IShapeFactory>();
            containerBuilder.RegisterInstance(_groupServiceMock.Object).As<IGroupService>();
            containerBuilder.RegisterInstance(_mailServiceMock.Object).As<IMailService>();

            var container = containerBuilder.Build();

            _controller = container.Resolve<GroupUsersController>();
            _controller.T = NullLocalizer.Instance;
        }

        [Test]
        public void TestUnauthorizedAccess() {
            _authorizerMock.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(false);

            var result = _controller.Index(default(PagerParameters));

            result.Should().BeOfType<HttpUnauthorizedResult>();
        }

        [Test]
        public void TestPostFilter() {
            var result = _controller.Index(3);

            result.Should().BeOfType<RedirectToRouteResult>();

            var redirectToRouteResult = (RedirectToRouteResult) result;
            redirectToRouteResult.RouteValues["action"].Should().Be("Index");
        }

        [Test]
        public void TestPostResendUserInvitationMails() {
            var result = _controller.Index("returnUrl", 3);

            result.Should().BeOfType<RedirectToRouteResult>();

            var redirectToRouteResult = (RedirectToRouteResult) result;
            redirectToRouteResult.RouteValues["action"].Should().Be("ConfirmResendUserInvitationMails");
            redirectToRouteResult.RouteValues["selectedGroupId"].Should().Be(3);
            redirectToRouteResult.RouteValues["returnUrl"].Should().Be("returnUrl");
        }

        [Test]
        public void TestPostResendUserInvitationMailsWithoutGroupId() {
            var result = _controller.Index("returnUrl");

            result.Should().BeOfType<RedirectToRouteResult>();

            var redirectToRouteResult = (RedirectToRouteResult)result;
            redirectToRouteResult.RouteValues["action"].Should().Be("Index");

            _notifierMock.Verify(x => x.Add(NotifyType.Warning, new LocalizedString("You must select a group. No mails were sent.")));
        }

        [Test]
        public void TestConfirmResendUserInvitationMails() {
            _groupServiceMock.Setup(x => x.GetGroups()).Returns(new[] {
                new GroupViewModel { Id = 1 },
                new GroupViewModel { Id = 1 },
                new GroupViewModel { Id = 3, Name = "TestGroup" }
            });

            var result = _controller.ConfirmResendUserInvitationMails("returnUrl", 3);

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            viewResult.Model.Should().BeOfType<ConfirmResendUserInvitationMailsViewModel>();

            var viewModel = (ConfirmResendUserInvitationMailsViewModel) viewResult.Model;
            viewModel.GroupId.Should().Be(3);
            viewModel.GroupName.Should().Be("TestGroup");
            viewModel.ReturnUrl.Should().Be("returnUrl");
        }

        [Test]
        public void TestPostConfirmResendUserInvitationMails() {
            _groupServiceMock.Setup(x => x.GetGroups()).Returns(new[] {
                new GroupViewModel { Id = 1 },
                new GroupViewModel { Id = 1 },
                new GroupViewModel { Id = 3, Name = "TestGroup", LogoUrl = "abc" }
            });

            var pendingEnglishUser = new UserMockFactory().Create("john.doe", "john.doe@example.com", "John", "Doe", "en", GroupMembershipStatus.Pending);
            var pendingFrenchUser = new UserMockFactory().Create("pierre", "pierre@example.com", "Pierre", "Bof", "fr", GroupMembershipStatus.Pending);
            var approvedEnglishUser = new UserMockFactory().Create("jane.doe", "jane.doe@example.com", "Jane", "Doe", "en", GroupMembershipStatus.Approved);
            var approvedFrenchUser = new UserMockFactory().Create("jeanne.d'arc", "jeanne@example.com", "Jeanne", "D'arc", "fr", GroupMembershipStatus.Approved);

            _groupServiceMock.Setup(x => x.GetUsersInGroup(3)).Returns(new[] {
                pendingEnglishUser,
                pendingFrenchUser,
                approvedEnglishUser,
                approvedFrenchUser
            });

            IEnumerable<IUser> englishRecipients = null;
            _mailServiceMock
                .Setup(x => x.SendUserInvitationMails("en", It.IsAny<IEnumerable<IUser>>(), It.IsAny<Func<string, string>>(), "TestGroup", "abc"))
                .Callback((string culture, IEnumerable<IUser> users, Func<string, string> createUrl, string groupName, string groupLogoUrl) => { englishRecipients = users; });

            IEnumerable<IUser> frenchRecipients = null;
            _mailServiceMock
                .Setup(x => x.SendUserInvitationMails("fr", It.IsAny<IEnumerable<IUser>>(), It.IsAny<Func<string, string>>(), "TestGroup", "abc"))
                .Callback((string culture, IEnumerable<IUser> users, Func<string, string> createUrl, string groupName, string groupLogoUrl) => { frenchRecipients = users; });

            var result = _controller.ConfirmResendUserInvitationMails(3, "returnUrl");

            result.Should().BeOfType<RedirectResult>();

            var redirectToRouteResult = (RedirectResult)result;
            redirectToRouteResult.Url.Should().Be("returnUrl");

            englishRecipients.Single().Should().Be(pendingEnglishUser);
            frenchRecipients.Single().Should().Be(pendingFrenchUser);

            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("The invitation mails have been sent.")));
        }
    }
}