using System.Linq;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class FeedReadModelGenerator :
        IFeedReadModelGenerator {
        private readonly IRepository<ChatRecord> _chatRepository;
        private readonly IRepository<FeedItemRecord> _feedItemRepository;
        private readonly IFindOtherUsersInGroupThatPossiblyOwnObjectQuery _findOtherUsersInGroupThatPossiblyOwnObjectQuery;
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly IGetUserByIdQuery _userQuery;

        public FeedReadModelGenerator(
            IRepository<FeedItemRecord> feedItemRepository,
            IGetUserByIdQuery userQuery,
            IFindOtherUsersInGroupThatPossiblyOwnObjectQuery findOtherUsersInGroupThatPossiblyOwnObjectQuery,
            IRepository<ObjectRequestRecord> objectRequestRepository,
            IRepository<ChatRecord> chatRepository) {
            _feedItemRepository = feedItemRepository;
            _userQuery = userQuery;
            _findOtherUsersInGroupThatPossiblyOwnObjectQuery = findOtherUsersInGroupThatPossiblyOwnObjectQuery;
            _objectRequestRepository = objectRequestRepository;
            _chatRepository = chatRepository;
        }

        public void Handle(ChatMessageAdded e) {
            var chat = _chatRepository.Fetch(x => x.ChatId == e.SourceId).Single();
            var objectRequest = _objectRequestRepository.Fetch(x => x.AggregateId == chat.ObjectRequestId).Single();
            var receivingUserId = chat.RequestingUserId == e.UserId ? chat.ConfirmingUserId : chat.RequestingUserId;
            var sendingUser = _userQuery.GetResult(e.UserId);

            var feedItem = new FeedItemRecord {
                DateTime = e.DateTime,
                Description = objectRequest.Description,
                ExtraInfo = objectRequest.ExtraInfo,
                ItemType = FeedItemType.ChatMessage,
                ObjectRequestId = objectRequest.AggregateId,
                SendingUserName = sendingUser?.GetUserDisplayName() ?? "",
                UserId = receivingUserId,
                ChatId = chat.ChatId
            };

            _feedItemRepository.Update(feedItem);
        }

        public void Handle(ObjectRequested e) {
            if (e.Status != ObjectRequestStatus.None) return;

            var user = _userQuery.GetResult(e.UserId);
            var otherUsersInGroup = _findOtherUsersInGroupThatPossiblyOwnObjectQuery.GetResults(e.UserId, e.Description);

            foreach (var otherUser in otherUsersInGroup) {
                var feedItem = new FeedItemRecord {
                    DateTime = e.CreatedDateTime,
                    Description = e.Description,
                    ExtraInfo = e.ExtraInfo,
                    ItemType = FeedItemType.ObjectRequest,
                    ObjectRequestId = e.SourceId,
                    SendingUserName = user?.GetUserDisplayName() ?? "",
                    UserId = otherUser.Id
                };

                _feedItemRepository.Update(feedItem);
            }
        }

        public void Handle(ObjectRequestConfirmed e) {
            var feedItems = _feedItemRepository.Fetch(x => x.ObjectRequestId == e.SourceId).ToList();

            foreach (var feedItemRecord in feedItems) {
                feedItemRecord.ConfirmationCount += 1;
                _feedItemRepository.Update(feedItemRecord);
            }
        }
    }
}