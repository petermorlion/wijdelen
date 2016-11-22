using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.CommandHandlers {
    public class ChatCommandHandler : ICommandHandler<StartChat> {
        private readonly IEventSourcedRepository<Chat> _repository;

        public ChatCommandHandler(IEventSourcedRepository<Chat> repository) {
            _repository = repository;
        }

        public void Handle(StartChat startChat) {
            var chat = new Chat(startChat.ChatId, startChat.ObjectRequestId, startChat.RequestingUserId, startChat.ConfirmingUserId);
            _repository.Save(chat, startChat.Id.ToString());
        }
    }
}