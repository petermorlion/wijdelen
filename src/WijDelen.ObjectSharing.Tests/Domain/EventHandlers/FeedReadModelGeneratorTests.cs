using System;
using System.Collections.Generic;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class FeedReadModelGeneratorTests {
        private FeedReadModelGenerator _generator;
        private Mock<IRepository<FeedItemRecord>> _feedItemRepositoryMock;
        private IList<FeedItemRecord> _persistedItems;
        private IUser _moe;
        private IUser _carl;
        private IUser _lenny;
        private readonly Guid _existingChatId = Guid.NewGuid();
        private readonly Guid _existingObjectRequestId = Guid.NewGuid();
        private FeedItemRecord _feedItemRecord;
        private FeedItemRecord _otherFeedItemRecord;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();

            _persistedItems = new List<FeedItemRecord>();
            _feedItemRepositoryMock = new Mock<IRepository<FeedItemRecord>>();
            _feedItemRepositoryMock.Setup(x => x.Update(It.IsAny<FeedItemRecord>())).Callback((FeedItemRecord i) => { _persistedItems.Add(i); });

            var userFactory = new UserFactory();
            _moe = userFactory.Create("moe.szyslak@simpsons.com", "moe.szyslak@simpsons.com", "Moe", "Szyslak");
            _carl = userFactory.Create("carl@simpsons.com", "carl@simpsons.com", "Carl", "Carlson");
            _lenny = userFactory.Create("lenny@simpsons.com", "lenny@simpsons.com", "Lenny", "Lenford");

            var userQueryMock = new Mock<IGetUserByIdQuery>();
            userQueryMock.Setup(x => x.GetResult(_moe.Id)).Returns(_moe);
            userQueryMock.Setup(x => x.GetResult(_carl.Id)).Returns(_carl);

            var otherUsersQueryMock = new Mock<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();
            otherUsersQueryMock.Setup(x => x.GetResults(_moe.Id, "Sneakers")).Returns(new[] {_carl, _lenny});

            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();
            chatRepositoryMock.SetRecords(new[] {
                new ChatRecord {
                    ChatId = _existingChatId,
                    RequestingUserId = _moe.Id,
                    RequestingUserName = _moe.GetUserDisplayName(),
                    ConfirmingUserId = _carl.Id,
                    ConfirmingUserName = _carl.GetUserDisplayName(),
                    ObjectRequestId = _existingObjectRequestId
                }
            });

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = _existingObjectRequestId,
                    Description = "Flaming Moe",
                    ExtraInfo = "For drinking"
                }
            });

            _feedItemRecord = new FeedItemRecord {
               ObjectRequestId = _existingObjectRequestId,
               ConfirmationCount = 2,
               UserId = _lenny.Id
            };

            _otherFeedItemRecord = new FeedItemRecord {
                ObjectRequestId = Guid.NewGuid(),
                ConfirmationCount = 1
            };

            _feedItemRepositoryMock.SetRecords(new[] {_feedItemRecord, _otherFeedItemRecord});

            builder.RegisterInstance(otherUsersQueryMock.Object).As<IFindOtherUsersInGroupThatPossiblyOwnObjectQuery>();
            builder.RegisterInstance(userQueryMock.Object).As<IGetUserByIdQuery>();
            builder.RegisterInstance(_feedItemRepositoryMock.Object).As<IRepository<FeedItemRecord>>();
            builder.RegisterInstance(chatRepositoryMock.Object).As<IRepository<ChatRecord>>();
            builder.RegisterInstance(objectRequestRepositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            builder.RegisterType<FeedReadModelGenerator>();

            var container = builder.Build();

            _generator = container.Resolve<FeedReadModelGenerator>();
        }

        [Test]
        public void WhenObjectRequested_ShouldAddFeedItem() {
            var e = new ObjectRequested {
                CreatedDateTime = new DateTime(2107, 9, 18, 10, 8, 12, DateTimeKind.Utc),
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                SourceId = Guid.NewGuid(),
                Status = ObjectRequestStatus.None,
                UserId = _moe.Id
            };

            _generator.Handle(e);

            _persistedItems.ShouldBeEquivalentTo(new List<FeedItemRecord> {
                new FeedItemRecord {
                    DateTime = new DateTime(2107, 9, 18, 10, 8, 12, DateTimeKind.Utc),
                    Description = "Sneakers",
                    ExtraInfo = "For sneaking",
                    ItemType = FeedItemType.ObjectRequest,
                    ObjectRequestId = e.SourceId,
                    ChatId = null,
                    ConfirmationCount = 0,
                    SendingUserName = "Moe Szyslak",
                    UserId = _carl.Id
                },
                new FeedItemRecord {
                    DateTime = new DateTime(2107, 9, 18, 10, 8, 12, DateTimeKind.Utc),
                    Description = "Sneakers",
                    ExtraInfo = "For sneaking",
                    ItemType = FeedItemType.ObjectRequest,
                    ObjectRequestId = e.SourceId,
                    ChatId = null,
                    ConfirmationCount = 0,
                    SendingUserName = "Moe Szyslak",
                    UserId = _lenny.Id
                }
            }, config => {
                return config.Excluding(x => x.SelectedMemberInfo.Name == "Id");
            });
        }

        [Test]
        public void WhenObjectRequestedAndBlockedForForbiddenWords_ShouldNotAddFeedItem() {
            var e = new ObjectRequested
            {
                CreatedDateTime = new DateTime(2107, 9, 18, 10, 8, 12, DateTimeKind.Utc),
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                SourceId = Guid.NewGuid(),
                Status = ObjectRequestStatus.BlockedForForbiddenWords,
                UserId = _moe.Id
            };

            _generator.Handle(e);

            _persistedItems.Should().BeEmpty();
        }

        [Test]
        public void WhenChatMessageAdded_ShouldAddFeedItem() {
            var e = new ChatMessageAdded {
                SourceId = _existingChatId,
                DateTime = new DateTime(2107, 9, 18, 10, 8, 12, DateTimeKind.Utc),
                UserId = _carl.Id
            };

            _generator.Handle(e);

            _persistedItems.ShouldBeEquivalentTo(new List<FeedItemRecord> {
                new FeedItemRecord {
                    DateTime = new DateTime(2107, 9, 18, 10, 8, 12, DateTimeKind.Utc),
                    Description = "Flaming Moe",
                    ExtraInfo = "For drinking",
                    ItemType = FeedItemType.ChatMessage,
                    ObjectRequestId = _existingObjectRequestId,
                    ChatId = e.SourceId,
                    ConfirmationCount = 0,
                    SendingUserName = "Carl Carlson",
                    UserId = _moe.Id
                }
            }, config => {
                return config.Excluding(x => x.SelectedMemberInfo.Name == "Id");
            });
        }

        [Test]
        public void WhenObjectRequestConfirmed_ShouldUpdateChatCount() {
            var e = new ObjectRequestConfirmed {
                SourceId = _existingObjectRequestId,
                ConfirmingUserId = _lenny.Id,
                DateTimeConfirmed = new DateTime(2107, 9, 18, 10, 8, 12, DateTimeKind.Utc)
            };

            _generator.Handle(e);

            _feedItemRecord.ConfirmationCount.Should().Be(3);
            _otherFeedItemRecord.ConfirmationCount.Should().Be(1);
        }

        [Test]
        public void WhenObjectRequestConfirmed_ShouldMarkAsConfirmed() {
            var e = new ObjectRequestConfirmed {
                SourceId = _existingObjectRequestId,
                ConfirmingUserId = _lenny.Id,
                DateTimeConfirmed = new DateTime(2107, 9, 18, 10, 8, 12, DateTimeKind.Utc)
            };

            _generator.Handle(e);

            _feedItemRecord.IsConfirmed.Should().BeTrue();
        }

        [Test]
        public void WhenObjectRequestDenied_ShouldRemoveFeedItemForUser() {
            var e = new ObjectRequestDenied {
                SourceId = _existingObjectRequestId,
                DenyingUserId = _lenny.Id
            };

            _generator.Handle(e);

            _feedItemRepositoryMock.Verify(x => x.Delete(_feedItemRecord));
        }

        [Test]
        public void WhenObjectRequestDeniedForNow_ShouldRemoveFeedItemForUser() {
            var e = new ObjectRequestDeniedForNow {
                SourceId = _existingObjectRequestId,
                DenyingUserId = _lenny.Id
            };

            _generator.Handle(e);

            _feedItemRepositoryMock.Verify(x => x.Delete(_feedItemRecord));
        }

        [Test]
        public void WhenChatStarted_ShouldMarkAsConfirmed() {
            var chatId = Guid.NewGuid();
            var e = new ChatStarted {
                ObjectRequestId = _existingObjectRequestId,
                ConfirmingUserId = _lenny.Id,
                SourceId = chatId
            };

            _generator.Handle(e);

            _feedItemRecord.ChatId.Should().Be(chatId);
        }
    }
}