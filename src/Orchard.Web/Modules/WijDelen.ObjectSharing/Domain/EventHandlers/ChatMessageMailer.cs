using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Infrastructure;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ChatMessageMailer : IEventHandler<ChatMessageAdded> {
        private readonly IRepository<ChatRecord> _chatRepository;
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly IGetUserByIdQuery _userQuery;
        private readonly IMailService _mailService;

        public ChatMessageMailer(IRepository<ChatRecord> chatRepository, IRepository<ObjectRequestRecord> objectRequestRepository, IGetUserByIdQuery userQuery, IMailService mailService) {
            _chatRepository = chatRepository;
            _objectRequestRepository = objectRequestRepository;
            _userQuery = userQuery;
            _mailService = mailService;
        }

        public void Handle(ChatMessageAdded e) {
            var chat = _chatRepository.Get(x => x.ChatId == e.ChatId);
            var objectRequest = _objectRequestRepository.Get(x => x.AggregateId == chat.ObjectRequestId);

            var fromUserName = e.UserId == chat.ConfirmingUserId ? chat.ConfirmingUserName : chat.RequestingUserName;

            var toUserName = e.UserId == chat.ConfirmingUserId ? chat.RequestingUserName : chat.ConfirmingUserName;
            var toUserId = e.UserId == chat.ConfirmingUserId ? chat.RequestingUserId : chat.ConfirmingUserId;
            var toUser = _userQuery.GetResult(toUserId);

            _mailService.SendChatMessageAddedMail(fromUserName, toUserName, objectRequest.Description, toUser.Email, e.ChatId, e.Message);
        }
    }
}