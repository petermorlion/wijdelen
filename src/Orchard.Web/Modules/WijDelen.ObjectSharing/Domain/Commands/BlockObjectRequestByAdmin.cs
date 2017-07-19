using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class BlockObjectRequestByAdmin : ICommand
    {

        public BlockObjectRequestByAdmin(Guid objectRequestId, string reason)
        {
            ObjectRequestId = objectRequestId;
            Reason = reason;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public Guid ObjectRequestId { get; }
        public string Reason { get; }
    }
}