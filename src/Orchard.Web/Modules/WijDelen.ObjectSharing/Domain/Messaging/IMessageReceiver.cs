using Orchard;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    /// <summary>
    /// Receives messages of events and dispatches them to the appropriate event handlers.
    /// </summary>
    public interface IMessageReceiver : IDependency {
        void Start();
        void Stop();
    }
}