using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ChatMessageReadModelGenerator : IEventHandler<ChatMessageAdded> {
        private readonly IRepository<ChatMessageRecord> _repository;
        private readonly IGetUserByIdQuery _userQuery;

        public ChatMessageReadModelGenerator(IRepository<ChatMessageRecord> repository, IGetUserByIdQuery userQuery) {
            _repository = repository;
            _userQuery = userQuery;
        }

        public void Handle(ChatMessageAdded e) {
            var user = _userQuery.GetResult(e.UserId);

            var newRecord = new ChatMessageRecord {
                ChatId = e.ChatId,
                DateTime = e.DateTime,
                Message = e.Message,
                UserId = e.UserId,
                UserName = user.UserName
            };

            _repository.Create(newRecord);
        }
    }
}