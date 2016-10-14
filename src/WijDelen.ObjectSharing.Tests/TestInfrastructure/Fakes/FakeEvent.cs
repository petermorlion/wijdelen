using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes {
    public class FakeEvent : IEvent {
        public Guid SourceId { get; }
    }
}