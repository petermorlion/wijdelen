using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.ViewModels.Admin;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ObjectRequestDetailsAdminControllerTests {
        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            _repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            _aggregateId = Guid.NewGuid();
            _repositoryMock.Setup(x => x.Table).Returns(new List<ObjectRequestRecord> {
                new ObjectRequestRecord {
                    Id = 12,
                    AggregateId = _aggregateId,
                    Status = ObjectRequestStatus.BlockedForForbiddenWords.ToString(),
                    CreatedDateTime = new DateTime(2017, 07, 19, 12, 30, 0, DateTimeKind.Utc),
                    Description = "Description test",
                    ExtraInfo = "Extra Info",
                    GroupName = "Group",
                    UserId = 33
                }
            }.AsQueryable());

            var userMockFactory = new UserFactory();
            _userMock = userMockFactory.Create("jane.doe@gmail.com", "jane.doe@gmail.com", "Jane", "Doe");
            _userQueryMock = new Mock<IGetUserByIdQuery>();
            _userQueryMock.Setup(x => x.GetResult(33)).Returns(_userMock);

            _notifierMock = new Mock<INotifier>();
            _commandHandlerMock = new Mock<ICommandHandler<BlockObjectRequestByAdmin>>();

            builder.RegisterInstance(_repositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            builder.RegisterInstance(_userQueryMock.Object).As<IGetUserByIdQuery>();
            builder.RegisterInstance(_notifierMock.Object).As<INotifier>();
            builder.RegisterInstance(_commandHandlerMock.Object).As<ICommandHandler<BlockObjectRequestByAdmin>>();
            builder.RegisterType<ObjectRequestDetailsAdminController>();

            var container = builder.Build();
            _controller = container.Resolve<ObjectRequestDetailsAdminController>();

            _controller.T = NullLocalizer.Instance;
        }

        private ObjectRequestDetailsAdminController _controller;
        private Mock<IRepository<ObjectRequestRecord>> _repositoryMock;
        private Mock<IGetUserByIdQuery> _userQueryMock;
        private IUser _userMock;
        private Guid _aggregateId;
        private Mock<INotifier> _notifierMock;
        private Mock<ICommandHandler<BlockObjectRequestByAdmin>> _commandHandlerMock;

        [Test]
        public void TestIndex() {
            var result = _controller.Index(_aggregateId);

            result.Should().BeOfType<ViewResult>();
            var model = result.As<ViewResult>().Model.As<ObjectRequestDetailsAdminViewModel>();
            model.Status.Should().Be("Blocked (forbidden words)");
            model.ForbiddenWords.ShouldBeEquivalentTo(new List<string> {"test"});
            model.CreatedDateTime.Should().Be(new DateTime(2017, 07, 19, 12, 30, 0, DateTimeKind.Utc).ToLocalTime());
            model.Description.Should().Be("Description test");
            model.ExtraInfo.Should().Be("Extra Info");
            model.GroupName.Should().Be("Group");
            model.FirstName.Should().Be("Jane");
            model.LastName.Should().Be("Doe");
        }

        [Test]
        public void TestIndexPost() {
            var result = _controller.Index(_aggregateId, "Just because");

            result.Should().BeOfType<RedirectToRouteResult>();
            var redirectToRouteResult = result.As<RedirectToRouteResult>();
            redirectToRouteResult.RouteValues["action"].Should().Be("Index");
            redirectToRouteResult.RouteValues["objectRequestId"].Should().Be(_aggregateId);

            _notifierMock.Verify(x => x.Add(NotifyType.Success, new LocalizedString("The request was blocked and a mail was sent to the user.")));

            _commandHandlerMock.Verify(x => x.Handle(It.Is<BlockObjectRequestByAdmin>(command => command.Reason == "Just because" && command.ObjectRequestId == _aggregateId)));
        }
    }
}