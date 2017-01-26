using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using WijDelen.Reports.Controllers;
using WijDelen.Reports.Models;
using WijDelen.Reports.Queries;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Tests.Controllers {
    [TestFixture]
    public class AdminControllerTests {
        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT()
        {
            var controller = new AdminController(default(ITotalsQuery));
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void ShouldReturnViewWithViewModel() {
            var totalsQueryMock = new Mock<ITotalsQuery>();
            totalsQueryMock.Setup(x => x.GetResults()).Returns(new Totals {
                Groups = 15,
                Users = 800,
                ObjectRequests = 200
            });

            var controller = new AdminController(totalsQueryMock.Object);

            var result = controller.Index();

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            viewResult.Model.Should().BeOfType<DashboardViewModel>();

            var viewModel = (DashboardViewModel) viewResult.Model;
            viewModel.TotalGroups.Should().Be(15);
            viewModel.TotalUsers.Should().Be(800);
            viewModel.TotalObjectRequests.Should().Be(200);
        }
    }
}