using System;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    public class SendingMessageEventArgs : EventArgs {
        public SendingMessageEventArgs(Message message) {
            Message = message;
        }

        public Message Message { get; private set; }
    }
}