using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
using Orchard.Localization.Providers;
using Orchard.Security;
using Orchard.UI.Notify;
using WijDelen.MailChimp;
using WijDelen.UserImport.Controllers;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.Tests.Mocks;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Tests.Controllers {
    [TestFixture]
    public class AccountControllerTests {
        private AccountController _controller;
        private Mock<IUpdateUserDetailsService> _updateUserDetailsServiceMock;
        private IUser _userMock;
        private Mock<INotifier> _notifierMock;
        private Mock<ICultureStorageProvider> _cultureStorageProviderMock;
        private Mock<IMailChimpClient> _mailChimpClientMock;
        private MockWorkContext _mockWorkContext;

        [SetUp]
        public void Init() {
            _mockWorkContext = new MockWorkContext();
            _userMock = new UserMockFactory().Create("peter.morlion@gmail.com", "peter.morlion@gmail.com", "Peter", "Morlion", "en-US", GroupMembershipStatus.Approved);
            _mockWorkContext.CurrentUser = _userMock;
            var orchardServicesMock = new Mock<IOrchardServices>();
            orchardServicesMock.Setup(x => x.WorkContext).Returns(_mockWorkContext);

            _updateUserDetailsServiceMock = new Mock<IUpdateUserDetailsService>();
            _cultureStorageProviderMock = new Mock<ICultureStorageProvider>();

            _notifierMock = new Mock<INotifier>();
            orchardServicesMock.Setup(x => x.Notifier).Returns(_notifierMock.Object);

            _mailChimpClientMock = new Mock<IMailChimpClient>();
            _mailChimpClientMock.Setup(x => x.IsSubscribed("peter.morlion@gmail.com")).Returns(true);

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<AccountController>();
            containerBuilder.RegisterInstance(orchardServicesMock.Object).As<IOrchardServices>();
            containerBuilder.RegisterInstance(_updateUserDetailsServiceMock.Object).As<IUpdateUserDetailsService>();
            containerBuilder.RegisterInstance(_notifierMock.Object).As<INotifier>();
            containerBuilder.RegisterInstance(_cultureStorageProviderMock.Object).As<ICultureStorageProvider>();
            containerBuilder.RegisterInstance(_mailChimpClientMock.Object).As<IMailChimpClient>();

            var container = containerBuilder.Build();

            _controller = container.Resolve<AccountController>();
            _controller.T = NullLocalizer.Instance;
        }

        [Test]
        public void TestIndex() {
            var result = _controller.Index();

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsInstanceOf<UserDetailsViewModel>(((ViewResult) result).Model);

            var viewmodel = ((ViewResult) result).Model as UserDetailsViewModel;
            Assert.IsNotNull(viewmodel);
            viewmodel.FirstName.Should().Be("Peter");
            viewmodel.LastName.Should().Be("Morlion");
            viewmodel.Culture.Should().Be("en-US");
            viewmodel.ReceiveMails.Should().BeTrue();
            viewmodel.IsSubscribedToNewsletter.Should().BeTrue();
        }

        [Test]
        public void TestIndexPost() {
            var viewModel = new UserDetailsViewModel {FirstName = "Moe", LastName = "Szyslak", Culture = "nl-BE", ReceiveMails = false, IsSubscribedToNewsletter = true};

            var result = _controller.Index(viewModel);

            ((RedirectToRouteResult) result).RouteValues["action"].Should().Be("Index");
            _updateUserDetailsServiceMock.Verify(x => x.UpdateUserDetails(_userMock, "Moe", "Szyslak", "nl-BE", false, true));
            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Your details have been saved successfully.")));
            _cultureStorageProviderMock.Verify(x => x.SetCulture("nl-BE"));
            _mockWorkContext.CurrentCulture.Should().Be("nl-BE");
        }

        [Test]
        public void TestIndexPostWithoutCulture() {
            var viewModel = new UserDetailsViewModel {
                FirstName = "Moe",
                LastName = "Szyslak",
                Culture = ""
            };

            var result = _controller.Index(viewModel);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult) result).ViewName);
            Assert.AreEqual("You must specify a language.", ((ViewResult) result).ViewData.ModelState["Culture"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestIndexPostWithoutFirstName() {
            var viewModel = new UserDetailsViewModel {
                FirstName = "",
                LastName = "Szyslak",
                Culture = "nl-BE"
            };

            var result = _controller.Index(viewModel);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult) result).ViewName);
            Assert.AreEqual("You must specify a first name.", ((ViewResult) result).ViewData.ModelState["FirstName"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestIndexPostWithoutLastName() {
            var viewModel = new UserDetailsViewModel {
                FirstName = "Moe",
                LastName = "",
                Culture = "nl-BE"
            };

            var result = _controller.Index(viewModel);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult) result).ViewName);
            Assert.AreEqual("You must specify a last name.", ((ViewResult) result).ViewData.ModelState["LastName"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestUnsubscribe() {
            var result = _controller.Unsubscribe();

            ((RedirectToRouteResult)result).RouteValues["action"].Should().Be("Index");
            _updateUserDetailsServiceMock.Verify(x => x.UpdateUserDetails(_userMock, "Peter", "Morlion", "en-US", false, null));
            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("You will no longer receive mails regarding requests or chat messages.")));
        }
    }
}