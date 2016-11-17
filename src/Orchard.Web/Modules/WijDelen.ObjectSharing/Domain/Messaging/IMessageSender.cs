using Orchard;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    /// <summary>
    /// Sends messages for the IMessageReceiver to be picked up.
    /// </summary>
    public interface IMessageSender : IDependency {
        void Send(Message message);
    }
}