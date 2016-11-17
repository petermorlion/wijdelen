using Orchard.Environment;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing {
    public class OrchardShellEvents : IOrchardShellEvents {
        private readonly IMessageReceiver _messageReceiver;

        public OrchardShellEvents(IMessageReceiver messageReceiver) {
            _messageReceiver = messageReceiver;
        }

        public void Activated() {
            _messageReceiver.Start();
        }

        public void Terminating() {
            //_messageReceiver.Stop();
        }
    }
}