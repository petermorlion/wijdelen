using System;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    /// <summary>
    /// Message sender that sends messages in memory through .NET events. To be used in conjunction with the InMemoryMessageReceiver.
    /// This means the messages are received synchronously and can't be scheduled.
    /// </summary>
    public class InMemoryMessageSender : IMessageSender {
        public event EventHandler<SendingMessageEventArgs> SendingMessage = delegate { };

        public void Send(Message message) {
            SendingMessage(this, new SendingMessageEventArgs(message));
        }
    }
}