using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class RequestObject : ICommand {

        public RequestObject(string description, string extraInfo) {
            Id = Guid.NewGuid();
            ObjectRequestId = Guid.NewGuid();
            Description = description;
            ExtraInfo = extraInfo;
        }

        public Guid Id { get; }
        public Guid ObjectRequestId { get; }

        public string Description { get; }

        public string ExtraInfo { get; }
    }
}