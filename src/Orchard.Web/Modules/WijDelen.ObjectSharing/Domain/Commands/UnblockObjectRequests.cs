using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class UnblockObjectRequests : ICommand {

        public UnblockObjectRequests(IEnumerable<Guid> objectRequestIds) {
            ObjectRequestIds = objectRequestIds;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public IEnumerable<Guid> ObjectRequestIds { get; }
    }
}