using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
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

        [SetUp]
        public void Init() {
            var mockWorkContext = new MockWorkContext();
            _userMock = new UserMockFactory().Create("peter.morlion@gmail.com", "peter.morlion@gmail.com", "Peter", "Morlion", "en-US", GroupMembershipStatus.Approved);
            mockWorkContext.CurrentUser = _userMock;
            var orchardServicesMock = new Mock<IOrchardServices>();
            orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);

            _updateUserDetailsServiceMock = new Mock<IUpdateUserDetailsService>();

            _notifierMock = new Mock<INotifier>();
            orchardServicesMock.Setup(x => x.Notifier).Returns(_notifierMock.Object);

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<AccountController>();
            containerBuilder.RegisterInstance(orchardServicesMock.Object).As<IOrchardServices>();
            containerBuilder.RegisterInstance(_updateUserDetailsServiceMock.Object).As<IUpdateUserDetailsService>();
            containerBuilder.RegisterInstance(_notifierMock.Object).As<INotifier>();

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
        }

        [Test]
        public void TestIndexPost() {
            var viewModel = new UserDetailsViewModel {FirstName = "Moe", LastName = "Szyslak", Culture = "nl-BE", ReceiveMails = false};

            var result = _controller.Index(viewModel);

            ((RedirectToRouteResult) result).RouteValues["action"].Should().Be("Index");
            _updateUserDetailsServiceMock.Verify(x => x.UpdateUserDetails(_userMock, "Moe", "Szyslak", "nl-BE", false));
            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("Your details have been saved successfully.")));
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
            _updateUserDetailsServiceMock.Verify(x => x.UpdateUserDetails(_userMock, "Peter", "Morlion", "en-US", false));
            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("You will no longer receive mails regarding requests or chat messages.")));
        }

        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a
        /// runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var localizer = NullLocalizer.Instance;

            _controller.T = localizer;

            Assert.AreEqual(localizer, _controller.T);
        }
    }
}