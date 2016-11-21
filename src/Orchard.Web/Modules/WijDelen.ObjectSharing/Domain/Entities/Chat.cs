using System;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Entities {
    /// <summary>
    /// Represents the chat between two users regarding an object request.
    /// </summary>
    public class Chat : EventSourced {
        private Chat(Guid id) : base(id) {}

        public Chat(Guid id, Guid objectRequestId, int requestingUserId, int confirmingUserId) : base(id) {
            // TODO: use events
            ObjectRequestId = objectRequestId;
            RequestingUserId = requestingUserId;
            ConfirmingUserId = confirmingUserId;
        }

        public Guid ObjectRequestId { get; private set; }
        public int RequestingUserId { get; private set; }
        public int ConfirmingUserId { get; private set; }
    }
}