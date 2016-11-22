using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class AddChatMessage : ICommand
    {
        public Guid Id { get; }
        public Guid ChatId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }

        public AddChatMessage(Guid chatId, int userId, string message, DateTime dateTime) {
            Id = Guid.NewGuid();
            ChatId = chatId;
            UserId = userId;
            Message = message;
            DateTime = dateTime;
        }
    }
}