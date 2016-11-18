using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class ConfirmObjectRequest : ICommand {
        public ConfirmObjectRequest(int confirmingUserId, Guid objectRequestId) {
            Id = Guid.NewGuid();
            ConfirmingUserId = confirmingUserId;
            ObjectRequestId = objectRequestId;
        }

        public Guid Id { get; }
        public int ConfirmingUserId { get; }
        public Guid ObjectRequestId { get; }
    }
}