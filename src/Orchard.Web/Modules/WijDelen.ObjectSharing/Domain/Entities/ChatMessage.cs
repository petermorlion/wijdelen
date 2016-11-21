using System;

namespace WijDelen.ObjectSharing.Domain.Entities {
    public class ChatMessage {
        public DateTime DateTime { get; private set; }
        public int UserId { get; private set; }
        public string Message { get; private set; }

        public ChatMessage(DateTime dateTime, int userId, string message) {
            DateTime = dateTime;
            UserId = userId;
            Message = message;
        }
    }
}