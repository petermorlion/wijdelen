using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class StopObjectRequest : ICommand {
        public Guid ObjectRequestId { get; }

        public Guid Id { get; }

        public StopObjectRequest(Guid objectRequestId) {
            ObjectRequestId = objectRequestId;
            Id = Guid.NewGuid();
        }
    }
}