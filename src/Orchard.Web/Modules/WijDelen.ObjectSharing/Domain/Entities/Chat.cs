using System;
using System.Collections.Generic;
using System.Linq;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Entities {
    /// <summary>
    /// Represents the chat between two users regarding an object request.
    /// </summary>
    public class Chat : EventSourced {
        private readonly IList<ChatMessage> _messages = new List<ChatMessage>();

        private Chat(Guid id) : base(id) {
            Handles<ChatStarted>(OnChatStarted);
            Handles<ChatMessageAdded>(OnChatMessageAdded);
        }

        public Chat(Guid id, Guid objectRequestId, int requestingUserId, int confirmingUserId) : this(id) {
            Update(new ChatStarted {ConfirmingUserId = confirmingUserId, RequestingUserId = requestingUserId, ObjectRequestId = objectRequestId});
        }

        public Chat(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        public void AddMessage(DateTime dateTime, int userId, string message) {
            Update(new ChatMessageAdded {ChatId = Id, DateTime = dateTime, Message = message, UserId = userId});
        }

        private void OnChatStarted(ChatStarted chatStarted) {
            ObjectRequestId = chatStarted.ObjectRequestId;
            RequestingUserId = chatStarted.RequestingUserId;
            ConfirmingUserId = chatStarted.ConfirmingUserId;
        }

        private void OnChatMessageAdded(ChatMessageAdded chatMessageAdded) {
            _messages.Add(new ChatMessage(chatMessageAdded.DateTime, chatMessageAdded.UserId, chatMessageAdded.Message));
        }

        public Guid ObjectRequestId { get; private set; }
        public int RequestingUserId { get; private set; }
        public int ConfirmingUserId { get; private set; }

        public IList<ChatMessage> Messages => _messages.OrderBy(x => x.DateTime).ToList();
    }
}