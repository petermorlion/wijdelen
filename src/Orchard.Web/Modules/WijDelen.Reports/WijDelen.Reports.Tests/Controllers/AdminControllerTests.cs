using System;
using System.Collections.Generic;
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
            var controller = new AdminController(default(ITotalsQuery), default(IMonthSummaryQuery), default(IDateTimeProvider), default(IGroupMonthSummaryQuery));
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
            monthSummaryQueryMock.Setup(x => x.GetResults(2017, 1)).Returns(thisMonthSummary);
            monthSummaryQueryMock.Setup(x => x.GetResults(2016, 12)).Returns(previousMonthSummary);

            var groupMonthSummaryQueryMock = new Mock<IGroupMonthSummaryQuery>();
            var groupMonthSummaries = new List<GroupMonthSummaryViewModel> {
                new GroupMonthSummaryViewModel { GroupName = "Small", ObjectRequestCount = 2 },
                new GroupMonthSummaryViewModel { GroupName = "Large", ObjectRequestCount = 200 }
            };
            groupMonthSummaryQueryMock.Setup(x => x.GetResults(2017, 1)).Returns(groupMonthSummaries);

            var controller = new AdminController(totalsQueryMock.Object, monthSummaryQueryMock.Object, dateTimeProviderMock.Object, groupMonthSummaryQueryMock.Object);

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
            viewModel.ThisMonth.Should().Be(new DateTime(2017, 1, 1));
            viewModel.PreviousMonth.Should().Be(new DateTime(2016, 12, 1));
            viewModel.GroupMonthSummaries.Should().BeEquivalentTo(groupMonthSummaries).And.BeInDescendingOrder(x => x.ObjectRequestCount);
        }
    }
}