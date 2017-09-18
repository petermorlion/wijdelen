using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class FeedControllerTests {
        private FeedController _controller;
        private IUser _user;
        private FeedItemRecord _item1;
        private FeedItemRecord _item2;

        [SetUp]
        public void Init()
        {
            var builder = new ContainerBuilder();

            var userFactory = new UserFactory();
            _user = userFactory.Create("homer@example.com", "homer@example.com", "Homer", "Simpson");

            var fakeOrchardServices = new FakeOrchardServices();
            fakeOrchardServices.WorkContext.CurrentUser = _user;

            _item1 = new FeedItemRecord {UserId = _user.Id};
            _item2 = new FeedItemRecord {UserId = _user.Id};
            var item3 = new FeedItemRecord {UserId = 66};
            var repositoryMock = new Mock<IRepository<FeedItemRecord>>();
            repositoryMock.SetRecords(new[] {_item1, _item2, item3});

            builder.RegisterInstance(fakeOrchardServices).As<IOrchardServices>();
            builder.RegisterInstance(repositoryMock.Object).As<IRepository<FeedItemRecord>>();
            builder.RegisterType<FeedController>();
            
            var container = builder.Build();
            _controller = container.Resolve<FeedController>();

            _controller.T = NullLocalizer.Instance;
        }

        [Test]
        public void Index_ShouldReturnRecords() {
            var result = _controller.Index();

            result.Should().BeOfType<ViewResult>();

            var indexViewModel = ((ViewResult) result).Model as FeedViewModel;
            indexViewModel.Should().NotBeNull();

            indexViewModel.Items.ShouldBeEquivalentTo(new[] {_item1, _item2});
        }
    }
}