using Orchard;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    public interface IEventBus : IDependency {
        void Publish(IEvent e);
    }
}