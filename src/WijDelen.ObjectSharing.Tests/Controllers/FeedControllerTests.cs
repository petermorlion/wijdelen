using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.ObjectSharing.ViewModels.Feed;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class FeedControllerTests {
        private FeedController _controller;
        private IUser _user;
        private IFeedItemViewModel _item1;
        private IFeedItemViewModel _item2;

        [SetUp]
        public void Init()
        {
            var builder = new ContainerBuilder();

            var userFactory = new UserFactory();
            _user = userFactory.Create("homer@example.com", "homer@example.com", "Homer", "Simpson");

            var groupMock = new Mock<IContent>();
            var myGroupId = 20;
            groupMock.Setup(x => x.Id).Returns(myGroupId);
            _user.As<GroupMembershipPart>().Group = groupMock.Object;

            var fakeOrchardServices = new FakeOrchardServices();
            fakeOrchardServices.WorkContext.CurrentUser = _user;

            _item1 = Mock.Of<IFeedItemViewModel>();
            _item2 = Mock.Of<IFeedItemViewModel>();

            var findFeedViewModelsQueryMock = new Mock<IFindFeedViewModelsQuery>();
            findFeedViewModelsQueryMock
                .Setup(x => x.GetResults(myGroupId, _user.Id, 20))
                .Returns(new[] {_item1, _item2});

            builder.RegisterInstance(fakeOrchardServices).As<IOrchardServices>();
            builder.RegisterInstance(findFeedViewModelsQueryMock.Object).As<IFindFeedViewModelsQuery>();
            builder.RegisterType<FeedController>();


            var container = builder.Build();
            _controller = container.Resolve<FeedController>();

            _controller.T = NullLocalizer.Instance;
        }

        [Test]
        public void Index_ShouldReturnOpenObjectRequestsInGroup() {
            var result = _controller.Index();

            result.Should().BeOfType<ViewResult>();

            var indexViewModel = ((ViewResult) result).Model as IndexViewModel;
            indexViewModel.Should().NotBeNull();

            indexViewModel.Items.ShouldBeEquivalentTo(new[] {_item1, _item2});
        }
    }
}