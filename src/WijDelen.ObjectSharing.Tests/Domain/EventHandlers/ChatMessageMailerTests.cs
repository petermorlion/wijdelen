using System;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Infrastructure;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ChatMessageMailerTests {
        [Test]
        public void WhenMessageAdded() {
            var chatId = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();

            var e = new ChatMessageAdded {
                ChatId = chatId,
                UserId = 22,
                Message = "I have Reebok Pumps"
            };

            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();
            chatRepositoryMock.SetRecords(new[] {
                new ChatRecord {
                    ChatId = chatId,
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

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Email).Returns("jane.doe@gmail.com");
            var userQueryMock = new Mock<IGetUserByIdQuery>();
            userQueryMock.Setup(x => x.GetResult(1)).Returns(userMock.Object);

            var mailServiceMock = new Mock<IMailService>();

            var handler = new ChatMessageMailer(chatRepositoryMock.Object, objectRequestRepositoryMock.Object, userQueryMock.Object, mailServiceMock.Object);

            handler.Handle(e);

            mailServiceMock.Verify(x => x.SendChatMessageAddedMail("John", "Jane", "Sneakers", "jane.doe@gmail.com", chatId, "I have Reebok Pumps"));
        }
    }
}