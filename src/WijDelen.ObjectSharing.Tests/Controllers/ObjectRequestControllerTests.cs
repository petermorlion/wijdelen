using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Localization;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
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
            var controller = new ObjectRequestController(null);
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void ShouldValidateNewObjectRequest() {
            var controller = new ObjectRequestController(null);
            var viewModel = new NewObjectRequestViewModel();

            var viewResult = controller.New(viewModel);

            ((ViewResult) viewResult).ViewData.ModelState["Description"].Errors.Single().ErrorMessage.Should().Be("Please provide a description of the item you need.");
            ((ViewResult) viewResult).ViewData.ModelState["ExtraInfo"].Errors.Single().ErrorMessage.Should().Be("Please provide some extra info.");
        }

        [Test]
        public void WhenPosting_ShouldCallCommandHandler()
        {
            var commandHandlerMock = new Mock<ICommandHandler<RequestObject>>();
            RequestObject command = null;
            commandHandlerMock.Setup(x => x.Handle(It.IsAny<RequestObject>())).Callback((RequestObject c) => command = c);

            var controller = new ObjectRequestController(commandHandlerMock.Object);
            var viewModel = new NewObjectRequestViewModel
            {
                Description = "Sneakers",
                ExtraInfo = "For sneaking"
            };

            var viewResult = controller.New(viewModel);

            ((ViewResult)viewResult).ViewData.ModelState.IsValid.Should().BeTrue();

            command.Description.Should().Be("Sneakers");
            command.ExtraInfo.Should().Be("For sneaking");
        }
    }
}