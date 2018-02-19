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
    public class ConfirmResendUserInvitationMailControllerTests {
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

            _orchardServicesMock.Setup(x => x.Notifier).Returns(_notifierMock.Object);

            builder.RegisterInstance(_orchardServicesMock.Object).As<IOrchardServices>();
            builder.RegisterInstance(_authorizerMock.Object).As<IAuthorizer>();
            builder.RegisterInstance(_membershipServiceMock.Object).As<IMembershipService>();
            builder.RegisterInstance(_mailServiceMock.Object).As<IMailService>();
            builder.RegisterInstance(_userImportServiceMock.Object).As<IUserImportService>();
            builder.RegisterInstance(_groupServiceMock.Object).As<IGroupService>();
            builder.RegisterInstance(_notifierMock.Object).As<INotifier>();
            builder.RegisterInstance(_cultureManagerMock.Object).As<ICultureManager>();
            builder.RegisterType<ConfirmResendUserInvitationMailController>();

            _container = builder.Build();
            _controller = _container.Resolve<ConfirmResendUserInvitationMailController>();
        }

        private ConfirmResendUserInvitationMailController _controller;
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
        public void Index_ForUserWithoutGroup_ShouldNotifyAndReturn() {
            var userMock = new UserMockFactory().Create("moe", "moe@example.com", "Moe", "Szyslak", "fr", GroupMembershipStatus.Approved);

            var site = new Mock<ISite>();
            var mockWorkContext = new MockWorkContext {CurrentSite = site.Object};
            _orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            site.Setup(x => x.BaseUrl).Returns("baseUrl");
            _membershipServiceMock.Setup(x => x.GetUser("moe")).Returns(userMock);

            _groupServiceMock.Setup(x => x.GetGroupForUser(2)).Returns((GroupViewModel) null);

            var result = _controller.Index("moe", "returnUrl");

            Assert.IsInstanceOf<RedirectResult>(result);
            ((RedirectResult) result).Url.Should().Be("returnUrl");

            _notifierMock.Verify(x => x.Add(NotifyType.Warning, new LocalizedString("The user needs to be part of a group first.")));
        }

        [Test]
        public void Index_ForUserWithGroup_ShouldNotifyAndReturn() {
            var userMock = new UserMockFactory().Create("moe", "moe@example.com", "Moe", "Szyslak", "fr", GroupMembershipStatus.Approved);

            var site = new Mock<ISite>();
            var mockWorkContext = new MockWorkContext {CurrentSite = site.Object};
            _orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            site.Setup(x => x.BaseUrl).Returns("baseUrl");
            _membershipServiceMock.Setup(x => x.GetUser("moe")).Returns(userMock);

            _groupServiceMock.Setup(x => x.GetGroupForUser(userMock.Id)).Returns(new GroupViewModel());

            var result = _controller.Index("moe", "returnUrl");

            Assert.IsInstanceOf<ViewResult>(result);
            ((ViewResult) result).Model.ShouldBeEquivalentTo(new ConfirmResendUserInvitationMailViewModel {
                UserId = userMock.Id,
                UserName = "moe",
                ReturnUrl = "returnUrl"
            });
        }

        [Test]
        public void IndexPost_ShouldResendMail() {
            var userMock = new UserMockFactory().Create("moe", "moe@example.com", "Moe", "Szyslak", "fr", GroupMembershipStatus.Approved);

            var site = new Mock<ISite>();
            var mockWorkContext = new MockWorkContext {CurrentSite = site.Object};
            _orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            site.Setup(x => x.BaseUrl).Returns("baseUrl");
            _membershipServiceMock.Setup(x => x.GetUser("moe")).Returns(userMock);

            var groupViewModel = new GroupViewModel {Name = "Existing group", LogoUrl = "url"};
            _groupServiceMock.Setup(x => x.GetGroupForUser(userMock.Id)).Returns(groupViewModel);

            IList<IUser> importedUsers = null;
            _mailServiceMock
                .Setup(x => x.SendUserInvitationMails("fr", It.IsAny<IEnumerable<IUser>>(), It.IsAny<Func<string, string>>(), "Existing group", "url", "text"))
                .Callback((string culture, IEnumerable<IUser> r, Func<string, string> f, string gn, string gu, string ei) => importedUsers = r.ToList());

            var viewModel = new ConfirmResendUserInvitationMailViewModel {
                UserId = userMock.Id,
                UserName = userMock.UserName,
                ReturnUrl = "returnUrl",
                Text = "text"
            };

            var result = _controller.IndexPOST(viewModel);

            result.Should().BeOfType<RedirectResult>();
            ((RedirectResult) result).Url.Should().Be("returnUrl");

            importedUsers.Single().Should().Be(userMock);

            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("User invitation mail has been sent.")));
        }
    }
}