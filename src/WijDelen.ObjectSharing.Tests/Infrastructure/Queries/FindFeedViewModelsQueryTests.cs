using System;
using System.Collections.Generic;
using Autofac;
using FluentAssertions;
using Moq;
using NHibernate;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Environment.Configuration;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.ObjectSharing.ViewModels.Feed;

namespace WijDelen.ObjectSharing.Tests.Infrastructure.Queries {
    public class FindFeedViewModelsQueryTests {
        private int _userId;
        private ObjectRequestRecord _objectRequest1;
        private ObjectRequestRecord _objectRequest2;
        private ObjectRequestRecord _myObjectRequest;
        private ObjectRequestRecord _blockedObjectRequest;
        private ObjectRequestRecord _objectRequestInOtherGroup;
        private FindFeedViewModelsQuery _query;
        private int _groupId;
        private ChatMessageViewModel _chatMessageViewModel;

        [SetUp]
        public void Init()
        {
            var builder = new ContainerBuilder();

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();

            var userFactory = new UserFactory();
            _userId = 66;
            _groupId = 20;
            var moe = userFactory.Create("moe@example.com", "moe@example.com", "Moe", "Szyslak");
            var carl = userFactory.Create("carl@example.com", "carl@example.com", "Carl", "Carlson");
            
            var findUsersQueryMock = new Mock<IFindUsersByIdsQuery>();
            findUsersQueryMock.Setup(x => x.GetResult(new[] { carl.Id, moe.Id })).Returns(new[] { moe, carl });

            var transactionManagerMock = new Mock<ITransactionManager>();
            var sessionMock = new Mock<ISession>();
            transactionManagerMock.Setup(x => x.GetSession()).Returns(sessionMock.Object);
            var sqlQueryMock = new Mock<ISQLQuery>();
            sessionMock.Setup(x => x.CreateSQLQuery(It.IsAny<string>())).Returns(sqlQueryMock.Object);
            var chatQueryMock = new Mock<IQuery>();
            sqlQueryMock.Setup(x => x.SetParameter("userId", _userId)).Returns(chatQueryMock.Object);
            _chatMessageViewModel = new ChatMessageViewModel {
                ChatId = Guid.NewGuid(),
                DateTime = new DateTime(2017, 9, 12, 0, 0, 0, DateTimeKind.Utc),
                Description = "Ladder",
                UserName = "Lenny"
            };

            chatQueryMock.Setup(x => x.List<object[]>()).Returns(new[] {
                new object[] {
                    _chatMessageViewModel.ChatId,
                    _chatMessageViewModel.Description,
                    _chatMessageViewModel.DateTime,
                    _chatMessageViewModel.UserName
                }
            });

            _objectRequest1 = new ObjectRequestRecord
            {
                AggregateId = Guid.NewGuid(),
                Description = "Sneakers",
                CreatedDateTime = new DateTime(2017, 9, 10, 0, 0, 0, DateTimeKind.Utc),
                GroupId = _groupId,
                UserId = moe.Id,
                Status = ObjectRequestStatus.None.ToString(),
                ExtraInfo = "For sneaking"
            };

            _objectRequest2 = new ObjectRequestRecord
            {
                AggregateId = Guid.NewGuid(),
                Description = "Flaming Moe",
                CreatedDateTime = new DateTime(2017, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                GroupId = _groupId,
                UserId = carl.Id,
                Status = ObjectRequestStatus.None.ToString(),
                ExtraInfo = "For drinking"
            };

            _myObjectRequest = new ObjectRequestRecord
            {
                Description = "My object request",
                CreatedDateTime = new DateTime(2017, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                GroupId = _groupId,
                UserId = _userId,
                Status = ObjectRequestStatus.None.ToString(),
            };

            _blockedObjectRequest = new ObjectRequestRecord
            {
                Description = "Blocked",
                CreatedDateTime = new DateTime(2017, 9, 14, 0, 0, 0, DateTimeKind.Utc),
                GroupId = _groupId,
                Status = ObjectRequestStatus.BlockedForForbiddenWords.ToString(),
                UserId = moe.Id
            };

            _objectRequestInOtherGroup = new ObjectRequestRecord
            {
                Description = "Other group",
                CreatedDateTime = new DateTime(2017, 9, 14, 0, 0, 0, DateTimeKind.Utc),
                GroupId = 666,
                Status = ObjectRequestStatus.None.ToString(),
                UserId = moe.Id
            };

            var objectRequestRecords = new List<ObjectRequestRecord> { _objectRequest1, _objectRequest2, _blockedObjectRequest, _objectRequestInOtherGroup, _myObjectRequest };
            for (var i = 0; i < 20; i++)
            {
                objectRequestRecords.Add(new ObjectRequestRecord
                {
                    GroupId = _groupId,
                    CreatedDateTime = new DateTime(2017, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                    UserId = carl.Id,
                    Status = ObjectRequestStatus.None.ToString()
                });
            }

            objectRequestRepositoryMock.SetRecords(objectRequestRecords);

            var objectRequestResponseRepositoryMock = new Mock<IRepository<ObjectRequestResponseRecord>>();
            objectRequestResponseRepositoryMock.SetRecords(new[] {
                new ObjectRequestResponseRecord { ObjectRequestId = _objectRequest2.AggregateId, UserId = _userId, Response = ObjectRequestAnswer.Yes },
                new ObjectRequestResponseRecord { ObjectRequestId = _objectRequest1.AggregateId, UserId = carl.Id },
            });

            var shellSettings = new ShellSettings();

            builder.RegisterInstance(findUsersQueryMock.Object).As<IFindUsersByIdsQuery>();
            builder.RegisterInstance(objectRequestRepositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            builder.RegisterInstance(transactionManagerMock.Object).As<ITransactionManager>();
            builder.RegisterInstance(objectRequestResponseRepositoryMock.Object).As<IRepository<ObjectRequestResponseRecord>>();
            builder.RegisterInstance(shellSettings).AsSelf();
            builder.RegisterType<FindFeedViewModelsQuery>();

            var container = builder.Build();
            _query = container.Resolve<FindFeedViewModelsQuery>();
        }

        [Test]
        public void ShouldReturnOpenObjectRequestsInGroup()
        {
            var result = _query.GetResults(_groupId, _userId, 10);

            result.Count.Should().Be(10, "because we don't want to show the entire feed");

            ((ObjectRequestViewModel)result[0]).DateTime.Should().Be(_objectRequest2.CreatedDateTime.ToLocalTime());
            ((ObjectRequestViewModel)result[0]).Description.Should().Be("Flaming Moe");
            ((ObjectRequestViewModel)result[0]).UserName.Should().Be("Carl Carlson");
            ((ObjectRequestViewModel)result[0]).ChatCount.Should().Be(3);
            ((ObjectRequestViewModel)result[0]).ExtraInfo.Should().Be("For drinking");
            ((ObjectRequestViewModel)result[0]).ObjectRequestId.Should().Be(_objectRequest2.AggregateId);
            ((ObjectRequestViewModel)result[0]).CurrentUsersResponse.Should().Be(ObjectRequestAnswer.Yes);

            ((ChatMessageViewModel)result[1]).ChatId.Should().Be(_chatMessageViewModel.ChatId);
            ((ChatMessageViewModel)result[1]).DateTime.Should().Be(_chatMessageViewModel.DateTime.ToLocalTime());
            ((ChatMessageViewModel)result[1]).Description.Should().Be(_chatMessageViewModel.Description);
            ((ChatMessageViewModel)result[1]).UserName.Should().Be(_chatMessageViewModel.UserName);

            ((ObjectRequestViewModel)result[2]).DateTime.Should().Be(_objectRequest1.CreatedDateTime.ToLocalTime());
            ((ObjectRequestViewModel)result[2]).Description.Should().Be("Sneakers");
            ((ObjectRequestViewModel)result[2]).UserName.Should().Be("Moe Szyslak");
            ((ObjectRequestViewModel)result[2]).ChatCount.Should().Be(12);
            ((ObjectRequestViewModel)result[2]).ExtraInfo.Should().Be("For sneaking");
            ((ObjectRequestViewModel)result[2]).ObjectRequestId.Should().Be(_objectRequest1.AggregateId);
            ((ObjectRequestViewModel)result[2]).CurrentUsersResponse.Should().BeNull();

            result.Should().NotContain(x => x is ObjectRequestViewModel && ((ObjectRequestViewModel)x).Description == _blockedObjectRequest.Description);
            result.Should().NotContain(x => x is ObjectRequestViewModel && ((ObjectRequestViewModel)x).Description == _objectRequestInOtherGroup.Description);
            result.Should().NotContain(x => x is ObjectRequestViewModel && ((ObjectRequestViewModel)x).Description == _myObjectRequest.Description);
        }
    }
}