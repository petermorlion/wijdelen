using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Tests.Domain.Messaging.Fakes {
    public class FakeEvent : IEvent {
        public Guid SourceId { get; }
    }
}