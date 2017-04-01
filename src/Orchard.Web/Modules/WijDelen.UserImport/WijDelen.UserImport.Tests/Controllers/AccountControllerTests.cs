using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
using WijDelen.UserImport.Controllers;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.Tests.Mocks;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Tests.Controllers {
    [TestFixture]
    public class AccountControllerTests {
        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var controller = new AccountController(Mock.Of<IOrchardServices>(), Mock.Of<IUpdateUserDetailsService>());
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void TestIndex() {
            var mockWorkContext = new MockWorkContext();
            var userMock = new UserMockFactory().Create("peter.morlion@gmail.com", "peter.morlion@gmail.com", "Peter", "Morlion");
            mockWorkContext.CurrentUser = userMock;
            var orchardServicesMock = new Mock<IOrchardServices>();
            orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            var controller = new AccountController(orchardServicesMock.Object, Mock.Of<IUpdateUserDetailsService>());

            var result = controller.Index();

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsInstanceOf<UserDetailsViewModel>(((ViewResult)result).Model);

            var viewmodel = ((ViewResult) result).Model as UserDetailsViewModel;
            Assert.IsNotNull(viewmodel);
            Assert.AreEqual("Peter", viewmodel.FirstName);
            Assert.AreEqual("Morlion", viewmodel.LastName);
        }

        [Test]
        public void TestIndexPost() {
            var mockWorkContext = new MockWorkContext();
            var userMock = new UserMockFactory().Create("peter.morlion@gmail.com", "peter.morlion@gmail.com", "Peter", "Morlion");
            mockWorkContext.CurrentUser = userMock;
            var orchardServicesMock = new Mock<IOrchardServices>();
            orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            var updateUserDetailsServiceMock = new Mock<IUpdateUserDetailsService>();
            var controller = new AccountController(orchardServicesMock.Object, updateUserDetailsServiceMock.Object);
            var viewModel = new UserDetailsViewModel {FirstName = "Moe", LastName = "Szyslak"};

            var result = controller.Index(viewModel);

            ((RedirectToRouteResult)result).RouteValues["action"].Should().Be("Index");
            updateUserDetailsServiceMock.Verify(x => x.UpdateUserDetails(userMock, "Moe", "Szyslak"));
        }

        [Test]
        public void TestIndexPostWithoutFirstName()
        {
            var controller = new AccountController(Mock.Of<IOrchardServices>(), Mock.Of<IUpdateUserDetailsService>());
            controller.T = NullLocalizer.Instance;
            var viewModel = new UserDetailsViewModel {
                FirstName = "",
                LastName = "Szyslak"
            };

            var result = controller.Index(viewModel);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult)result).ViewName);
            Assert.AreEqual("You must specify a first name.", ((ViewResult)result).ViewData.ModelState["FirstName"].Errors.Single().ErrorMessage);
        }

        [Test]
        public void TestIndexPostWithoutLastName()
        {
            var controller = new AccountController(Mock.Of<IOrchardServices>(), Mock.Of<IUpdateUserDetailsService>());
            controller.T = NullLocalizer.Instance;
            var viewModel = new UserDetailsViewModel
            {
                FirstName = "Moe",
                LastName = ""
            };

            var result = controller.Index(viewModel);

            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("", ((ViewResult)result).ViewName);
            Assert.AreEqual("You must specify a last name.", ((ViewResult)result).ViewData.ModelState["LastName"].Errors.Single().ErrorMessage);
        }
    }
}