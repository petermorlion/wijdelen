using System;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.ViewModels.Admin;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ObjectRequestDetailsAdminControllerTests {
        private ObjectRequestDetailsAdminController _controller;
        private Mock<IRepository<ObjectRequestRecord>> _repositoryMock;
        private Mock<IGetUserByIdQuery> _userQueryMock;
        private IUser _userMock;

        [SetUp]
        public void Init()
        {
            var builder = new ContainerBuilder();

            _repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            _repositoryMock.Setup(x => x.Get(12)).Returns(new ObjectRequestRecord {
                Id = 12,
                Status = ObjectRequestStatus.BlockedForForbiddenWords.ToString(),
                CreatedDateTime = new DateTime(2017, 07, 19, 12, 30, 0, DateTimeKind.Utc),
                Description = "Description",
                ExtraInfo = "Extra Info",
                GroupName = "Group",
                UserId = 33
            });

            var userMockFactory = new UserFactory();
            _userMock = userMockFactory.Create("jane.doe@gmail.com", "jane.doe@gmail.com", "Jane", "Doe");
            _userQueryMock = new Mock<IGetUserByIdQuery>();
            _userQueryMock.Setup(x => x.GetResult(33)).Returns(_userMock);

            builder.RegisterInstance(_repositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            builder.RegisterInstance(_userQueryMock.Object).As<IGetUserByIdQuery>();
            builder.RegisterType<ObjectRequestDetailsAdminController>();

            var container = builder.Build();
            _controller = container.Resolve<ObjectRequestDetailsAdminController>();

            _controller.T = NullLocalizer.Instance;
        }

        [Test]
        public void TestIndex() {
            var result = _controller.Index(12);

            result.Should().BeOfType<ViewResult>();
            var model = result.As<ViewResult>().Model.As<ObjectRequestDetailsAdminViewModel>();
            model.Status.Should().Be("BlockedForForbiddenWords");
            model.CreatedDateTime.Should().Be(new DateTime(2017, 07, 19, 12, 30, 0, DateTimeKind.Utc).ToLocalTime());
            model.Description.Should().Be("Description");
            model.ExtraInfo.Should().Be("Extra Info");
            model.GroupName.Should().Be("Group");
            model.FirstName.Should().Be("Jane");
            model.LastName.Should().Be("Doe");
        }
    }
}