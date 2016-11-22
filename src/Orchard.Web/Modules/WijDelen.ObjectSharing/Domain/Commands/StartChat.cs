using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class StartChat : ICommand {
        public StartChat(Guid objectRequestId, int requestingUserId, int confirmingUserId) {
            ObjectRequestId = objectRequestId;
            RequestingUserId = requestingUserId;
            ConfirmingUserId = confirmingUserId;
            Id = Guid.NewGuid();
            ChatId = Guid.NewGuid();
        }

        public Guid Id { get; }
        public Guid ChatId { get; }
        public Guid ObjectRequestId { get; }
        public int RequestingUserId { get; }
        public int ConfirmingUserId { get; }
    }
}