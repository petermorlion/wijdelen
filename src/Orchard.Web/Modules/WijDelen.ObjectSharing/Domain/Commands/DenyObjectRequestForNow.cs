using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class DenyObjectRequestForNow : ICommand {
        public DenyObjectRequestForNow(int denyingUserId, Guid objectRequestId) {
            Id = Guid.NewGuid();
            DenyingUserId = denyingUserId;
            ObjectRequestId = objectRequestId;
        }

        public Guid Id { get; }
        public int DenyingUserId { get; }
        public Guid ObjectRequestId { get; }
    }
}