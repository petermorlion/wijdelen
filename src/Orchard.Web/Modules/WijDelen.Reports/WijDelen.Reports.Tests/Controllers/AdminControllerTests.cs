using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Autofac;
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
        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow()).Returns(new DateTime(2017, 1, 26, 0, 0, 0, DateTimeKind.Utc));

            _totalsQueryMock = new Mock<ITotalsQuery>();
            _monthSummaryQueryMock = new Mock<IMonthSummaryQuery>();
            _groupMonthSummaryQueryMock = new Mock<IGroupMonthSummaryQuery>();
            _dateLocalizationServicesMock = new Mock<IDateLocalizationServices>();
            _groupsQueryMock = new Mock<IGroupsQuery>();
            _groupDetailsQueryMock = new Mock<IGroupDetailsQuery>();
            _requestsDetailsQueryMock = new Mock<IRequestsDetailsQuery>();

            builder.RegisterInstance(dateTimeProviderMock.Object).As<IDateTimeProvider>();
            builder.RegisterInstance(_totalsQueryMock.Object).As<ITotalsQuery>();
            builder.RegisterInstance(_monthSummaryQueryMock.Object).As<IMonthSummaryQuery>();
            builder.RegisterInstance(_groupMonthSummaryQueryMock.Object).As<IGroupMonthSummaryQuery>();
            builder.RegisterInstance(_dateLocalizationServicesMock.Object).As<IDateLocalizationServices>();
            builder.RegisterInstance(_groupsQueryMock.Object).As<IGroupsQuery>();
            builder.RegisterInstance(_groupDetailsQueryMock.Object).As<IGroupDetailsQuery>();
            builder.RegisterInstance(_requestsDetailsQueryMock.Object).As<IRequestsDetailsQuery>();
            builder.RegisterType<AdminController>();

            var container = builder.Build();
            _controller = container.Resolve<AdminController>();
            _controller.T = NullLocalizer.Instance;
        }

        private AdminController _controller;
        private Mock<ITotalsQuery> _totalsQueryMock;
        private Mock<IMonthSummaryQuery> _monthSummaryQueryMock;
        private Mock<IGroupMonthSummaryQuery> _groupMonthSummaryQueryMock;
        private Mock<IGroupsQuery> _groupsQueryMock;
        private Mock<IGroupDetailsQuery> _groupDetailsQueryMock;
        private Mock<IDateLocalizationServices> _dateLocalizationServicesMock;
        private Mock<IRequestsDetailsQuery> _requestsDetailsQueryMock;

        [Test]
        public void WhenRequestingGroups_ShouldReturnDefaultView() {
            var groups = new List<GroupViewModel>();
            _groupsQueryMock
                .Setup(x => x.GetResults())
                .Returns(groups);

            var groupDetailsViewModels = new List<GroupDetailsViewModel>();
            _groupDetailsQueryMock
                .Setup(x => x.GetResults(null, new DateTime(2017, 1, 1), new DateTime(2017, 1, 31)))
                .Returns(groupDetailsViewModels);

            var result = _controller.Groups(null, null, null);

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (GroupsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2017, 1, 1));
            viewModel.StopDate.Should().Be(new DateTime(2017, 1, 31));
            viewModel.GroupDetails.ShouldBeEquivalentTo(groupDetailsViewModels);
            viewModel.Groups.ShouldBeEquivalentTo(new List<GroupViewModel> {
                new GroupViewModel {Id = 0, Name = ""}
            });
        }

        [Test]
        public void WhenRequestingGroupsForPeriodAndGroup_ShouldReturnViewForPeriodAndGroup() {
            var startDate = new DateTime(2015, 1, 1);
            var stopDate = new DateTime(2015, 1, 31);
            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("startDate", null)).Returns(startDate);
            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("stopDate", null)).Returns(stopDate);

            var groups = new List<GroupViewModel> {
                new GroupViewModel {Id = 1, Name = "Pin Pals"},
                new GroupViewModel {Id = 2, Name = "Flying Hellfish"}
            };

            _groupsQueryMock
                .Setup(x => x.GetResults())
                .Returns(groups);

            var groupDetailsViewModels = new List<GroupDetailsViewModel>();
            _groupDetailsQueryMock
                .Setup(x => x.GetResults(2, new DateTime(2017, 1, 1), new DateTime(2017, 1, 31)))
                .Returns(groupDetailsViewModels);

            var result = _controller.Groups(2, "startDate", "stopDate");

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (GroupsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2015, 1, 1));
            viewModel.StopDate.Should().Be(new DateTime(2015, 1, 31));
            viewModel.GroupDetails.ShouldBeEquivalentTo(groupDetailsViewModels);
            viewModel.Groups.ShouldBeEquivalentTo(new List<GroupViewModel> {
                new GroupViewModel {Id = 0, Name = ""},
                new GroupViewModel {Id = 2, Name = "Flying Hellfish"},
                new GroupViewModel {Id = 1, Name = "Pin Pals"}
            });
        }

        [Test]
        public void WhenRequestingGroupsWithOnlyStartDate_ShouldReturnViewWithStopDate() {
            var startDate = new DateTime(2015, 1, 15);

            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("startDate", null)).Returns(startDate);
            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("stopDate", null)).Returns((DateTime?) null);

            var groupDetailsViewModels = new List<GroupDetailsViewModel>();
            _groupDetailsQueryMock
                .Setup(x => x.GetResults(null, new DateTime(2017, 1, 1), new DateTime(2017, 1, 31)))
                .Returns(groupDetailsViewModels);

            var result = _controller.Groups(null, "startDate", "stopDate");

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (GroupsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2015, 1, 15));
            viewModel.StopDate.Should().Be(new DateTime(2015, 1, 31));
            viewModel.GroupDetails.ShouldBeEquivalentTo(groupDetailsViewModels);
        }

        [Test]
        public void WhenRequestingGroupsWithOnlyStopDate_ShouldReturnViewWithStartDate() {
            var stopDate = new DateTime(2015, 1, 15);

            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("startDate", null)).Returns((DateTime?) null);
            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("stopDate", null)).Returns(stopDate);

            var groupDetailsViewModels = new List<GroupDetailsViewModel>();
            _groupDetailsQueryMock
                .Setup(x => x.GetResults(null, new DateTime(2015, 1, 1), new DateTime(2015, 1, 31)))
                .Returns(groupDetailsViewModels);

            var result = _controller.Groups(null, "startDate", "stopDate");

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (GroupsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2015, 1, 1));
            viewModel.StopDate.Should().Be(new DateTime(2015, 1, 15));
            viewModel.GroupDetails.ShouldBeEquivalentTo(groupDetailsViewModels);
        }

        [Test]
        public void WhenRequestingOverview_ShouldReturnViewWithViewModel() {
            _totalsQueryMock.Setup(x => x.GetResults()).Returns(new Totals {
                Groups = 15,
                Users = 800,
                ObjectRequests = 200
            });

            var thisMonthSummary = new SummaryViewModel();
            var previousMonthSummary = new SummaryViewModel();
            _monthSummaryQueryMock.Setup(x => x.GetResults(2017, 1)).Returns(thisMonthSummary);
            _monthSummaryQueryMock.Setup(x => x.GetResults(2016, 12)).Returns(previousMonthSummary);

            var groupMonthSummaries = new List<GroupMonthSummaryViewModel> {
                new GroupMonthSummaryViewModel {GroupName = "Small", ObjectRequestCount = 2},
                new GroupMonthSummaryViewModel {GroupName = "Large", ObjectRequestCount = 200}
            };

            _groupMonthSummaryQueryMock.Setup(x => x.GetResults(2017, 1)).Returns(groupMonthSummaries);

            var result = _controller.Index();

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
        public void WhenRequestingRequests_ShouldReturnDefaultView() {
            var groups = new List<GroupViewModel>();
            _groupsQueryMock
                .Setup(x => x.GetResults())
                .Returns(groups);

            var result = _controller.Requests(null, null, 0);

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (RequestsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2017, 1, 1));
            viewModel.StopDate.Should().Be(new DateTime(2017, 1, 31));
            viewModel.Details.ShouldBeEquivalentTo(new List<RequestsDetailsViewModel>());
            viewModel.Groups.ShouldBeEquivalentTo(new List<GroupViewModel> {
                new GroupViewModel {Id = 0, Name = ""}
            });
        }

        [Test]
        public void WhenRequestingRequestsForPeriodAndGroup_ShouldReturnViewForPeriodAndGroup() {
            var startDate = new DateTime(2015, 1, 1);
            var stopDate = new DateTime(2015, 1, 31);
            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("startDate", null)).Returns(startDate);
            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("stopDate", null)).Returns(stopDate);

            var groups = new List<GroupViewModel> {
                new GroupViewModel {Id = 1, Name = "Pin Pals"},
                new GroupViewModel {Id = 2, Name = "Flying Hellfish"}
            };

            _groupsQueryMock
                .Setup(x => x.GetResults())
                .Returns(groups);

            var requestsDetailsViewModels = new List<RequestsDetailsViewModel>();
            _requestsDetailsQueryMock
                .Setup(x => x.GetResults(2, new DateTime(2015, 1, 1), new DateTime(2015, 1, 31)))
                .Returns(requestsDetailsViewModels);

            var result = _controller.Requests("startDate", "stopDate", 2);

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (RequestsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2015, 1, 1));
            viewModel.StopDate.Should().Be(new DateTime(2015, 1, 31));
            viewModel.Details.Should().BeSameAs(requestsDetailsViewModels);
            viewModel.Groups.ShouldBeEquivalentTo(new List<GroupViewModel> {
                new GroupViewModel {Id = 0, Name = ""},
                new GroupViewModel {Id = 2, Name = "Flying Hellfish"},
                new GroupViewModel {Id = 1, Name = "Pin Pals"}
            });
        }

        [Test]
        public void WhenRequestingRequestsWithOnlyStartDate_ShouldReturnViewWithStopDate() {
            var startDate = new DateTime(2015, 1, 15);

            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("startDate", null)).Returns(startDate);
            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("stopDate", null)).Returns((DateTime?) null);

            var requestDetailsViewModels = new List<RequestsDetailsViewModel>();
            _requestsDetailsQueryMock
                .Setup(x => x.GetResults(2, new DateTime(2015, 1, 1), new DateTime(2015, 1, 31)))
                .Returns(requestDetailsViewModels);

            var result = _controller.Requests("startDate", "stopDate", 2);

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (RequestsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2015, 1, 15));
            viewModel.StopDate.Should().Be(new DateTime(2015, 1, 31));
            viewModel.Details.ShouldBeEquivalentTo(requestDetailsViewModels);
        }

        [Test]
        public void WhenRequestingRequestsWithOnlyStopDate_ShouldReturnViewWithStartDate() {
            var stopDate = new DateTime(2015, 1, 15);

            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("startDate", null)).Returns((DateTime?) null);
            _dateLocalizationServicesMock.Setup(x => x.ConvertFromLocalizedDateString("stopDate", null)).Returns(stopDate);

            var requestDetailsViewModels = new List<RequestsDetailsViewModel>();
            _requestsDetailsQueryMock
                .Setup(x => x.GetResults(2, new DateTime(2015, 1, 1), new DateTime(2015, 1, 31)))
                .Returns(requestDetailsViewModels);

            var result = _controller.Requests("startDate", "stopDate", 2);

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult) result;
            var viewModel = (RequestsViewModel) viewResult.Model;
            viewModel.StartDate.Should().Be(new DateTime(2015, 1, 1));
            viewModel.StopDate.Should().Be(new DateTime(2015, 1, 15));
            viewModel.Details.ShouldBeEquivalentTo(requestDetailsViewModels);
        }
    }
}