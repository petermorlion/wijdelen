using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Entities {
    /// <summary>
    /// Represents the chat between two users regarding an object request.
    /// </summary>
    public class Chat : EventSourced {
        private Chat(Guid id) : base(id) {
            Handles<ChatStarted>(OnChatStarted);
        }

        public Chat(Guid id, Guid objectRequestId, int requestingUserId, int confirmingUserId) : this(id) {
            Update(new ChatStarted {ConfirmingUserId = confirmingUserId, RequestingUserId = requestingUserId, ObjectRequestId = objectRequestId});
        }

        public Chat(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        private void OnChatStarted(ChatStarted chatStarted) {
            ObjectRequestId = chatStarted.ObjectRequestId;
            RequestingUserId = chatStarted.RequestingUserId;
            ConfirmingUserId = chatStarted.ConfirmingUserId;
        }

        public Guid ObjectRequestId { get; private set; }
        public int RequestingUserId { get; private set; }
        public int ConfirmingUserId { get; private set; }
    }
}