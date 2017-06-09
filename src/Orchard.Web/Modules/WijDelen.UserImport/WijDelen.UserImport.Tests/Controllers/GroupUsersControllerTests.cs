using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using WijDelen.UserImport.Controllers;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.Tests.Mocks;

namespace WijDelen.UserImport.Tests.Controllers {
    [TestFixture]
    public class GroupUsersControllerTests {
        private GroupUsersController _controller;
        private Mock<IAuthorizer> _authorizerMock;

        [SetUp]
        public void Init() {
            var mockWorkContext = new MockWorkContext();
            _authorizerMock = new Mock<IAuthorizer>();

            var orchardServicesMock = new Mock<IOrchardServices>();
            orchardServicesMock.Setup(x => x.WorkContext).Returns(mockWorkContext);
            orchardServicesMock.Setup(x => x.Authorizer).Returns(_authorizerMock.Object);
            
            var siteServiceMock = new Mock<ISiteService>();
            var shapeFactoryMock = new Mock<IShapeFactory>();
            var groupServiceMock = new Mock<IGroupService>();
            
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<GroupUsersController>();
            containerBuilder.RegisterInstance(orchardServicesMock.Object).As<IOrchardServices>();
            containerBuilder.RegisterInstance(siteServiceMock.Object).As<ISiteService>();
            containerBuilder.RegisterInstance(shapeFactoryMock.Object).As<IShapeFactory>();
            containerBuilder.RegisterInstance(groupServiceMock.Object).As<IGroupService>();

            var container = containerBuilder.Build();

            _controller = container.Resolve<GroupUsersController>();
            _controller.T = NullLocalizer.Instance;
        }

        [Test]
        public void TestUnauthorizedAccess() {
            _authorizerMock.Setup(x => x.Authorize(StandardPermissions.SiteOwner, It.IsAny<LocalizedString>())).Returns(false);

            var result = _controller.Index(default(PagerParameters));

            result.Should().BeOfType<HttpUnauthorizedResult>();
        }
    }
}