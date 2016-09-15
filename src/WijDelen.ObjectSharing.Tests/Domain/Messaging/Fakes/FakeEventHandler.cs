using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Tests.Domain.Messaging.Fakes {
    public class FakeEventHandler : IEventHandler<FakeEvent> {
        public bool WasCalled { get; private set; }
        public void Handle(FakeEvent e) {
            WasCalled = true;
        }
    }
}