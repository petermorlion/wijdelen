using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.EventHandlers.Notifications;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;
using IMailService = WijDelen.ObjectSharing.Infrastructure.IMailService;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers.Notifications {
    [TestFixture]
    public class EmailNotificationServiceTests {
        private IUser _requestingUser;
        private IUser _otherUser;
        private IUser _unsubscribedUser;
        private Mock<IMailService> _mailServiceMock;
        private Mock<IGetUserByIdQuery> _getUserByIdQueryMock;
        private Mock<IGroupService> _groupServiceMock;
        private EmailNotificationService _service;
        private Mock<IRepository<ObjectRequestNotificationRecord>> _notificationRecordRepositoryMock;
        private ObjectRequestNotificationRecord _persistedRecord;

        [SetUp]
        public void Init() {
            var fakeUserFactory = new UserFactory();
            _requestingUser = fakeUserFactory.Create("jos", "jos@example.com", "Jos", "Joskens");
            _otherUser = fakeUserFactory.Create("peter.morlion", "peter.morlion@gmail.com", "Peter", "Morlion");
            _unsubscribedUser = fakeUserFactory.Create("john.doe@example.com", "john.doe@example.com", "John", "Doe", false);

            _mailServiceMock = new Mock<IMailService>();
            
            _getUserByIdQueryMock = new Mock<IGetUserByIdQuery>();
            _getUserByIdQueryMock.Setup(x => x.GetResult(_requestingUser.Id)).Returns(_requestingUser);

            _groupServiceMock = new Mock<IGroupService>();
            _groupServiceMock.Setup(x => x.GetGroupForUser(_requestingUser.Id)).Returns(new GroupViewModel {Name = "TestGroup"});

            _notificationRecordRepositoryMock = new Mock<IRepository<ObjectRequestNotificationRecord>>();
            _notificationRecordRepositoryMock
                .Setup(x => x.Create(It.IsAny<ObjectRequestNotificationRecord>()))
                .Callback((ObjectRequestNotificationRecord r) => _persistedRecord = r);

            var builder = new ContainerBuilder();
            builder.RegisterInstance(_mailServiceMock.Object).As<IMailService>();
            builder.RegisterInstance(_getUserByIdQueryMock.Object).As<IGetUserByIdQuery>();
            builder.RegisterInstance(_groupServiceMock.Object).As<IGroupService>();
            builder.RegisterInstance(_notificationRecordRepositoryMock.Object).As<IRepository<ObjectRequestNotificationRecord>>();
            builder.RegisterType<EmailNotificationService>();
            var container = builder.Build();

            _service = container.Resolve<EmailNotificationService>();
        }

        [Test]
        public void ObjectRequested_ShouldOnlySendToSubscribedUsers() {
            var sourceId = Guid.NewGuid();
            var createdDateTime = DateTime.UtcNow;

            IEnumerable<IUser> users = null;
            _mailServiceMock
                .Setup(x => x.SendObjectRequestMail("Jos Joskens", "TestGroup", sourceId, "sneakers", "for sneaking", It.IsAny<IEnumerable<IUser>>()))
                .Callback((string n, string g, Guid s, string d, string e, IEnumerable<IUser> u) => {
                    users = u;
                });

            _service.Handle(new []{_otherUser, _unsubscribedUser}, new ObjectRequested{SourceId = sourceId, Description = "sneakers", ExtraInfo = "for sneaking", UserId = _requestingUser.Id, CreatedDateTime = createdDateTime});

            users.Single().Should().Be(_otherUser);

            _persistedRecord.ShouldBeEquivalentTo(new ObjectRequestNotificationRecord
            {
                ObjectRequestId = sourceId,
                ReceivingUserId = _otherUser.Id,
                RequestingUserId = _requestingUser.Id,
                SentDateTime = DateTime.UtcNow
            }, options => options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 5000))
                .When(info => info.SelectedMemberPath == "SentDateTime"));
        }

        [Test]
        public void ObjectRequestUnblocked_ShouldOnlySendToSubscribedUsers() {
            var sourceId = Guid.NewGuid();

            IEnumerable<IUser> users = null;
            _mailServiceMock
                .Setup(x => x.SendObjectRequestMail("Jos Joskens", "TestGroup", sourceId, "sneakers", "for sneaking", It.IsAny<IEnumerable<IUser>>()))
                .Callback((string n, string g, Guid s, string d, string e, IEnumerable<IUser> u) => {
                    users = u;
                });

            _service.Handle(new []{_otherUser, _unsubscribedUser}, new ObjectRequestUnblocked{SourceId = sourceId, Description = "sneakers", ExtraInfo = "for sneaking", UserId = _requestingUser.Id});

            users.Single().Should().Be(_otherUser);

            _persistedRecord.ShouldBeEquivalentTo(new ObjectRequestNotificationRecord
            {
                ObjectRequestId = sourceId,
                ReceivingUserId = _otherUser.Id,
                RequestingUserId = _requestingUser.Id,
                SentDateTime = DateTime.UtcNow
            }, options => options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 5000))
                .When(info => info.SelectedMemberPath == "SentDateTime"));
        }
    }
}