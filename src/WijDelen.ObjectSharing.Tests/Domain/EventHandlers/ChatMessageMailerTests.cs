using System;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Infrastructure;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ChatMessageMailerTests {
        [SetUp]
        public void Init() {
            _chatId = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();

            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();
            chatRepositoryMock.SetRecords(new[] {
                new ChatRecord {
                    ChatId = _chatId,
                    ConfirmingUserId = 22,
                    ConfirmingUserName = "John",
                    RequestingUserId = 1,
                    RequestingUserName = "Jane",
                    ObjectRequestId = objectRequestId
                }
            });


            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = objectRequestId,
                    Description = "Sneakers"
                }
            });

            var userMockFactory = new UserFactory();
            _userMock = userMockFactory.Create("jane.doe@gmail.com", "jane.doe@gmail.com", "Jane", "Doe");
            _userMock.As<UserDetailsPart>().Culture = "test";
            
            var userQueryMock = new Mock<IGetUserByIdQuery>();
            userQueryMock.Setup(x => x.GetResult(1)).Returns(_userMock);

            _mailServiceMock = new Mock<IMailService>();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(chatRepositoryMock.Object).As<IRepository<ChatRecord>>();
            containerBuilder.RegisterInstance(objectRequestRepositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            containerBuilder.RegisterInstance(_mailServiceMock.Object).As<IMailService>();
            containerBuilder.RegisterInstance(userQueryMock.Object).As<IGetUserByIdQuery>();
            containerBuilder.RegisterType<ChatMessageMailer>();
            var container = containerBuilder.Build();

            _handler = container.Resolve<ChatMessageMailer>();
        }

        private ChatMessageMailer _handler;
        private Mock<IMailService> _mailServiceMock;
        private Guid _chatId;
        private IUser _userMock;

        [Test]
        public void WhenMessageAdded() {
            var e = new ChatMessageAdded {
                ChatId = _chatId,
                UserId = 22,
                Message = "I have Reebok Pumps"
            };

            _handler.Handle(e);

            _mailServiceMock.Verify(x => x.SendChatMessageAddedMail("test", "John", "Sneakers", "jane.doe@gmail.com", _chatId, "I have Reebok Pumps"));
        }

        [Test]
        public void WhenMessageAddedWithUnsubscribedUser() {
            _userMock.As<UserDetailsPart>().ReceiveMails = false;
            var e = new ChatMessageAdded
            {
                ChatId = _chatId,
                UserId = 22,
                Message = "I have Reebok Pumps"
            };

            _handler.Handle(e);

            _mailServiceMock.Verify(x => x.SendChatMessageAddedMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }
    }
}