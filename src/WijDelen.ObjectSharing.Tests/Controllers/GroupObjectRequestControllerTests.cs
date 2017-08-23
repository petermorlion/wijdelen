using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.ObjectSharing.ViewModels;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class GroupObjectRequestControllerTests {
        private GroupObjectRequestController _controller;
        private Guid _validObjectRequestIdForOtherUserInSameGroup1;
        private Guid _validObjectRequestIdForOtherUserInSameGroup2;
        private Guid _validObjectRequestIdForOtherUserInSameGroup3;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            var orchardServicesMock = new FakeOrchardServices();

            var groupMock = new Mock<IContent>();
            groupMock.Setup(x => x.Id).Returns(20);
            var fakeUserFactory = new UserFactory();
            var user = fakeUserFactory.Create("jane.doe@example.com", "jane.doe@example.com", "Jane", "Doe");
            user.As<GroupMembershipPart>().Group = groupMock.Object;
            orchardServicesMock.WorkContext.CurrentUser = user;

            var userQueryMock = new Mock<IFindUsersByIdsQuery>();
            var user1 = fakeUserFactory.Create("john.doe@example.com", "john.doe@example.com", "John", "Doe");
            var user2 = fakeUserFactory.Create("homer.simpson@example.com", "homer.simpson@example.com", "Homer", "Simpson");
            var user3 = fakeUserFactory.Create("abraham.simpson@example.com", "abraham.simpson@example.com", "Abraham", "Simpson");
            userQueryMock.Setup(x => x.GetResult(new[] { user1.Id, user2.Id, user3.Id })).Returns(new[] {
                user1,
                user2,
                user3
            });

            _validObjectRequestIdForOtherUserInSameGroup1 = Guid.NewGuid();
            _validObjectRequestIdForOtherUserInSameGroup2 = Guid.NewGuid();
            _validObjectRequestIdForOtherUserInSameGroup3 = Guid.NewGuid();
            var persistentRecords = new[] {
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid(),
                    UserId = user1.Id,
                    GroupId = 10,
                    Status = ObjectRequestStatus.None.ToString()
                },
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid(),
                    UserId = user1.Id,
                    GroupId = 20,
                    Status = ObjectRequestStatus.Stopped.ToString()
                },
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid(),
                    UserId = user.Id,
                    GroupId = 20
                },
                new ObjectRequestRecord {
                    AggregateId = _validObjectRequestIdForOtherUserInSameGroup1,
                    UserId = user1.Id,
                    GroupId = 20,
                    Description = "Item 1",
                    Status = ObjectRequestStatus.None.ToString()
                },
                new ObjectRequestRecord {
                    AggregateId = _validObjectRequestIdForOtherUserInSameGroup2,
                    UserId = user2.Id,
                    GroupId = 20,
                    Description = "Item 2",
                    Status = ObjectRequestStatus.None.ToString()
                },
                new ObjectRequestRecord {
                    AggregateId = _validObjectRequestIdForOtherUserInSameGroup3,
                    UserId = user3.Id,
                    GroupId = 20,
                    Description = "Item 3",
                    Status = ObjectRequestStatus.None.ToString()
                },
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid(),
                    UserId = 13,
                    GroupId = 20,
                    Status = ObjectRequestStatus.None.ToString()
                }
            };

            repositoryMock.SetRecords(persistentRecords);

            builder.RegisterInstance(repositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            builder.RegisterInstance(orchardServicesMock).As<IOrchardServices>();
            builder.RegisterInstance(userQueryMock.Object).As<IFindUsersByIdsQuery>();
            builder.RegisterType<GroupObjectRequestController>();

            var container = builder.Build();
            _controller = container.Resolve<GroupObjectRequestController>();
        }

        [Test]
        public void WhenGettingIndex_ShouldReturnThreeObjectRequestsForCurrentUsersGroup() {
            var result = _controller.Index();

            result.Should().BeOfType<ViewResult>();

            var model = ((ViewResult)result).Model as IEnumerable<GroupObjectRequestViewModel>;
            model.Should().NotBeNull();
            var viewModels = model.ToList();
            viewModels.ShouldBeEquivalentTo(new[] {
                new GroupObjectRequestViewModel {
                    Description = "Item 1",
                    FirstName = "John",
                    LastName = "Doe",
                    UserName = "john.doe@example.com"
                },
                new GroupObjectRequestViewModel {
                    Description = "Item 2",
                    FirstName = "Homer",
                    LastName = "Simpson",
                    UserName = "homer.simpson@example.com"
                },
                new GroupObjectRequestViewModel {
                    Description = "Item 3",
                    FirstName = "Abraham",
                    LastName = "Simpson",
                    UserName = "abraham.simpson@example.com"
                }
            });
        }
    }
}