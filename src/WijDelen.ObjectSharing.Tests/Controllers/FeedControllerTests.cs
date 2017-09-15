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
        private ObjectRequestRecord _objectRequest1;
        private IUser _user;
        private ObjectRequestRecord _objectRequest2;
        private ObjectRequestRecord _blockedObjectRequest;
        private ObjectRequestRecord _objectRequestInOtherGroup;
        private ObjectRequestRecord _myObjectRequest;

        [SetUp]
        public void Init()
        {
            var builder = new ContainerBuilder();

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();

            var userFactory = new UserFactory();
            _user = userFactory.Create("homer@example.com", "homer@example.com", "Homer", "Simpson");
            var moe = userFactory.Create("moe@example.com", "moe@example.com", "Moe", "Szyslak");
            var carl = userFactory.Create("carl@example.com", "carl@example.com", "Carl", "Carlson");

            var groupMock = new Mock<IContent>();
            var myGroupId = 20;
            groupMock.Setup(x => x.Id).Returns(myGroupId);
            _user.As<GroupMembershipPart>().Group = groupMock.Object;

            var fakeOrchardServices = new FakeOrchardServices();
            fakeOrchardServices.WorkContext.CurrentUser = _user;

            var findUsersQueryMock = new Mock<IFindUsersByIdsQuery>();
            findUsersQueryMock.Setup(x => x.GetResult(new[] { carl.Id, moe.Id })).Returns(new[] {moe, carl});

            builder.RegisterInstance(findUsersQueryMock.Object).As<IFindUsersByIdsQuery>();
            builder.RegisterInstance(objectRequestRepositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            builder.RegisterInstance(fakeOrchardServices).As<IOrchardServices>();
            builder.RegisterType<FeedController>();

            _objectRequest1 = new ObjectRequestRecord
            {
                AggregateId = Guid.NewGuid(),
                Description = "Sneakers",
                CreatedDateTime = new DateTime(2017, 9, 10, 0, 0, 0, DateTimeKind.Utc),
                GroupId = myGroupId,
                UserId = moe.Id,
                Status = ObjectRequestStatus.None.ToString(),
                ChatCount = 12
            };

            _objectRequest2 = new ObjectRequestRecord
            {
                AggregateId = Guid.NewGuid(),
                Description = "Flaming Moe",
                CreatedDateTime = new DateTime(2017, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                GroupId = myGroupId,
                UserId = carl.Id,
                Status = ObjectRequestStatus.None.ToString(),
                ChatCount = 3
            };

            _myObjectRequest = new ObjectRequestRecord
            {
                AggregateId = Guid.NewGuid(),
                Description = "Khlav Kalash",
                CreatedDateTime = new DateTime(2017, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                GroupId = myGroupId,
                UserId = _user.Id,
                Status = ObjectRequestStatus.None.ToString(),
                ChatCount = 4
            };

            _blockedObjectRequest = new ObjectRequestRecord
            {
                AggregateId = Guid.NewGuid(),
                Description = "Sex",
                CreatedDateTime = new DateTime(2017, 9, 14, 0, 0, 0, DateTimeKind.Utc),
                GroupId = 777,
                Status = ObjectRequestStatus.BlockedForForbiddenWords.ToString(),
                UserId = moe.Id
            };

            _objectRequestInOtherGroup = new ObjectRequestRecord
            {
                AggregateId = Guid.NewGuid(),
                Description = "Donuts",
                CreatedDateTime = new DateTime(2017, 9, 14, 0, 0, 0, DateTimeKind.Utc),
                GroupId = 666,
                Status = ObjectRequestStatus.None.ToString(),
                UserId = moe.Id
            };

            var objectRequestRecords = new List<ObjectRequestRecord> { _objectRequest1, _objectRequest2, _blockedObjectRequest, _objectRequestInOtherGroup, _myObjectRequest };
            for (var i = 0; i < 20; i++) {
                objectRequestRecords.Add(new ObjectRequestRecord {
                    GroupId = myGroupId,
                    CreatedDateTime = new DateTime(2017, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                    UserId = carl.Id
                });
            }
            
            objectRequestRepositoryMock.SetRecords(objectRequestRecords);

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

            indexViewModel.ObjectRequests.Count.Should().Be(10, "because we don't want to show the entire history of requests");

            indexViewModel.ObjectRequests[0].CreatedDateTime.Should().Be(_objectRequest2.CreatedDateTime.ToLocalTime());
            indexViewModel.ObjectRequests[0].Description.Should().Be("Flaming Moe");
            indexViewModel.ObjectRequests[0].UserName.Should().Be("Carl Carlson");
            indexViewModel.ObjectRequests[0].ChatCount.Should().Be(3);

            indexViewModel.ObjectRequests[1].CreatedDateTime.Should().Be(_objectRequest1.CreatedDateTime.ToLocalTime());
            indexViewModel.ObjectRequests[1].Description.Should().Be("Sneakers");
            indexViewModel.ObjectRequests[1].UserName.Should().Be("Moe Szyslak");
            indexViewModel.ObjectRequests[1].ChatCount.Should().Be(12);

            indexViewModel.ObjectRequests.Should().NotContain(x => x.Description == _blockedObjectRequest.Description);
            indexViewModel.ObjectRequests.Should().NotContain(x => x.Description == _objectRequestInOtherGroup.Description);
            indexViewModel.ObjectRequests.Should().NotContain(x => x.Description == _myObjectRequest.Description);
        }
    }
}