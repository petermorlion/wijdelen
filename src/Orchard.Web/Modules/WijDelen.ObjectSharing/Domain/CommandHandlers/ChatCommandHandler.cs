using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.CommandHandlers {
    public class ChatCommandHandler : ICommandHandler<StartChat>, ICommandHandler<AddChatMessage> {
        private readonly IEventSourcedRepository<Chat> _repository;

        public ChatCommandHandler(IEventSourcedRepository<Chat> repository) {
            _repository = repository;
        }

        public void Handle(StartChat startChat) {
            var chat = new Chat(startChat.ChatId, startChat.ObjectRequestId, startChat.RequestingUserId, startChat.ConfirmingUserId);
            _repository.Save(chat, startChat.Id.ToString());
        }

        public void Handle(AddChatMessage command) {
            var chat = _repository.Find(command.ChatId);
            chat.AddMessage(command.DateTime, command.UserId, command.Message);
            _repository.Save(chat, command.Id.ToString());
        }
    }
}