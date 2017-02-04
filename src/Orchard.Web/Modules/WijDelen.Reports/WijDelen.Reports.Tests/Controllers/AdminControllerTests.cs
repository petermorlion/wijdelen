using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Localization.Services;
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
            var controller = new AdminController(
                default(ITotalsQuery), 
                default(IMonthSummaryQuery), 
                default(IDateTimeProvider), 
                default(IGroupMonthSummaryQuery), 
                default(IDateLocalizationServices), 
                default(IGroupDetailsQuery),
                default(IGroupsQuery));
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void WhenRequestingOverview_ShouldReturnViewWithViewModel() {
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

            var controller = new AdminController(totalsQueryMock.Object, monthSummaryQueryMock.Object, dateTimeProviderMock.Object, groupMonthSummaryQueryMock.Object, default(IDateLocalizationServices), default(IGroupDetailsQuery), default(IGroupsQuery));

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

        [Test]
        public void WhenRequestingDetails_ShouldReturnDefaultView() {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow()).Returns(new DateTime(2017, 1, 26, 0, 0, 0, DateTimeKind.Utc));

            var dateLocalizationServicesMock = new Mock<IDateLocalizationServices>();
            dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("startDate", null)).Returns((DateTime?)null);
            dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("stopDate", null)).Returns((DateTime?)null);

            var groups = new List<GroupViewModel>();
            var groupsQueryMock = new Mock<IGroupsQuery>();
            groupsQueryMock
                .Setup(x => x.GetResults())
                .Returns(groups);

            var groupDetailsViewModels = new List<GroupDetailsViewModel>();
            var groupDetailsQueryMock = new Mock<IGroupDetailsQuery>();
            groupDetailsQueryMock
                .Setup(x => x.GetResults(null, new DateTime(2017, 1, 1), new DateTime(2017, 1, 31)))
                .Returns(groupDetailsViewModels);

            var controller = new AdminController(default(ITotalsQuery), default(IMonthSummaryQuery), dateTimeProviderMock.Object, default(IGroupMonthSummaryQuery), dateLocalizationServicesMock.Object, groupDetailsQueryMock.Object, groupsQueryMock.Object);

            var result = controller.Details(null, null, null);

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (DetailsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2017, 1, 1));
            viewModel.StopDate.Should().Be(new DateTime(2017, 1, 31));
            viewModel.GroupDetails.ShouldBeEquivalentTo(groupDetailsViewModels);
            viewModel.Groups.ShouldBeEquivalentTo(new List<GroupViewModel> {
                new GroupViewModel { Id = 0, Name = "" }
            });
        }

        [Test]
        public void WhenRequestingDetailsForPeriodAndGroup_ShouldReturnViewForPeriodAndGroup() {
            var startDate = new DateTime(2015, 1, 1);
            var stopDate = new DateTime(2015, 1, 31);

            var dateLocalizationServicesMock = new Mock<IDateLocalizationServices>();
            dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("startDate", null)).Returns(startDate);
            dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("stopDate", null)).Returns(stopDate);
            
            var groups = new List<GroupViewModel> {
                new GroupViewModel { Id = 1, Name = "Pin Pals" },
                new GroupViewModel { Id = 2, Name = "Flying Hellfish" }
            };

            var groupsQueryMock = new Mock<IGroupsQuery>();
            groupsQueryMock
                .Setup(x => x.GetResults())
                .Returns(groups);

            var groupDetailsViewModels = new List<GroupDetailsViewModel>();
            var groupDetailsQueryMock = new Mock<IGroupDetailsQuery>();
            groupDetailsQueryMock
                .Setup(x => x.GetResults(2, new DateTime(2017, 1, 1), new DateTime(2017, 1, 31)))
                .Returns(groupDetailsViewModels);

            var controller = new AdminController(default(ITotalsQuery), default(IMonthSummaryQuery), default(IDateTimeProvider), default(IGroupMonthSummaryQuery), dateLocalizationServicesMock.Object, groupDetailsQueryMock.Object, groupsQueryMock.Object);

            var result = controller.Details(2, "startDate", "stopDate");

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (DetailsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2015, 1, 1));
            viewModel.StopDate.Should().Be(new DateTime(2015, 1, 31));
            viewModel.GroupDetails.ShouldBeEquivalentTo(groupDetailsViewModels);
            viewModel.Groups.ShouldBeEquivalentTo(new List<GroupViewModel> {
                new GroupViewModel { Id = 0, Name = "" },
                new GroupViewModel { Id = 2, Name = "Flying Hellfish" },
                new GroupViewModel { Id = 1, Name = "Pin Pals" }
            });
        }

        [Test]
        public void WhenRequestingDetailsWithOnlyStartDate_ShouldReturnViewWithStopDate() {
            var startDate = new DateTime(2015, 1, 15);

            var dateLocalizationServicesMock = new Mock<IDateLocalizationServices>();
            dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("startDate", null)).Returns(startDate);
            dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("stopDate", null)).Returns((DateTime?)null);

            var groupsQueryMock = new Mock<IGroupsQuery>();

            var groupDetailsViewModels = new List<GroupDetailsViewModel>();
            var groupDetailsQueryMock = new Mock<IGroupDetailsQuery>();
            groupDetailsQueryMock
                .Setup(x => x.GetResults(null, new DateTime(2017, 1, 1), new DateTime(2017, 1, 31)))
                .Returns(groupDetailsViewModels);

            var controller = new AdminController(default(ITotalsQuery), default(IMonthSummaryQuery), default(IDateTimeProvider), default(IGroupMonthSummaryQuery), dateLocalizationServicesMock.Object, groupDetailsQueryMock.Object, groupsQueryMock.Object);

            var result = controller.Details(null, "startDate", "stopDate");

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (DetailsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2015, 1, 15));
            viewModel.StopDate.Should().Be(new DateTime(2015, 1, 31));
            viewModel.GroupDetails.ShouldBeEquivalentTo(groupDetailsViewModels);
        }

        [Test]
        public void WhenRequestingDetailsWithOnlyStopDate_ShouldReturnViewWithStartDate() {
            var stopDate = new DateTime(2015, 1, 15);

            var dateLocalizationServicesMock = new Mock<IDateLocalizationServices>();
            dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("startDate", null)).Returns((DateTime?) null);
            dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("stopDate", null)).Returns(stopDate);

            var groupsQueryMock = new Mock<IGroupsQuery>();

            var groupDetailsViewModels = new List<GroupDetailsViewModel>();
            var groupDetailsQueryMock = new Mock<IGroupDetailsQuery>();
            groupDetailsQueryMock
                .Setup(x => x.GetResults(null, new DateTime(2017, 1, 1), new DateTime(2017, 1, 31)))
                .Returns(groupDetailsViewModels);

            var controller = new AdminController(default(ITotalsQuery), default(IMonthSummaryQuery), default(IDateTimeProvider), default(IGroupMonthSummaryQuery), dateLocalizationServicesMock.Object, groupDetailsQueryMock.Object, groupsQueryMock.Object);

            var result = controller.Details(null, "startDate", "stopDate");

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (DetailsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2015, 1, 1));
            viewModel.StopDate.Should().Be(new DateTime(2015, 1, 15));
            viewModel.GroupDetails.ShouldBeEquivalentTo(groupDetailsViewModels);
        }
    }
}