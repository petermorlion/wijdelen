using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class ConfirmObjectRequest : ICommand {
        public ConfirmObjectRequest(int confirmingUserId) {
            Id = Guid.NewGuid();
            ConfirmingUserId = confirmingUserId;
        }

        public Guid Id { get; }
        public int ConfirmingUserId { get; }
    }
}