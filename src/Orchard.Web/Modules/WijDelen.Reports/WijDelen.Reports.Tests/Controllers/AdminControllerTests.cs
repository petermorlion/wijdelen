using System;
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
            var controller = new AdminController(default(ITotalsQuery), default(IMonthSummaryQuery), default(IDateTimeProvider));
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

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow()).Returns(new DateTime(2017, 1, 26, 0, 0, 0, DateTimeKind.Utc));

            var thisMonthSummary = new SummaryViewModel();
            var previousMonthSummary = new SummaryViewModel();
            var monthSummaryQueryMock = new Mock<IMonthSummaryQuery>();
            monthSummaryQueryMock.Setup(x => x.GetResults(1)).Returns(thisMonthSummary);
            monthSummaryQueryMock.Setup(x => x.GetResults(12)).Returns(previousMonthSummary);

            var controller = new AdminController(totalsQueryMock.Object, monthSummaryQueryMock.Object, dateTimeProviderMock.Object);

            var result = controller.Index();

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            viewResult.Model.Should().BeOfType<DashboardViewModel>();

            var viewModel = (DashboardViewModel) viewResult.Model;
            viewModel.TotalGroups.Should().Be(15);
            viewModel.TotalUsers.Should().Be(800);
            viewModel.TotalObjectRequests.Should().Be(200);
            viewModel.ThisMonthSummary.Should().Be(thisMonthSummary);
            viewModel.PreviousMonthSummary.Should().Be(previousMonthSummary);
        }
    }
}