using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Tests {
    [TestFixture]
    public class OrchardShellEventsTests {
        [Test]
        public void ShouldStartMessageReceiverOnActivating() {
            var messageReceiverMock = new Mock<IMessageReceiver>();
            var orchardShellEvents = new OrchardShellEvents(messageReceiverMock.Object);

            orchardShellEvents.Activated();

            messageReceiverMock.Verify(x => x.Start());
        }

        [Test]
        public void ShouldStopMessageReceiverOnTerminating()
        {
            var messageReceiverMock = new Mock<IMessageReceiver>();
            var orchardShellEvents = new OrchardShellEvents(messageReceiverMock.Object);

            orchardShellEvents.Terminating();

            messageReceiverMock.Verify(x => x.Stop());
        }
    }
}