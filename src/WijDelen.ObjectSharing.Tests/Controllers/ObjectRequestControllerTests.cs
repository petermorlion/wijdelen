using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ObjectRequestControllerTests {
        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT()
        {
            var controller = new ObjectRequestController();
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void ShouldValidateNewObjectRequest() {
            var controller = new ObjectRequestController();
            var viewModel = new NewObjectRequestViewModel();

            var viewResult = controller.New(viewModel);

            ((ViewResult) viewResult).ViewData.ModelState["Description"].Errors.Single().ErrorMessage.Should().Be("Please provide a description of the item you need.");
            ((ViewResult) viewResult).ViewData.ModelState["ExtraInfo"].Errors.Single().ErrorMessage.Should().Be("Please provide some extra info.");
        }
    }
}