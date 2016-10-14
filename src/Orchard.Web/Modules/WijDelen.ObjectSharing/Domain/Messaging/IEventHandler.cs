using Orchard;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    /// <summary>
    /// Marker interface that makes it easier to discover handlers via reflection.
    /// </summary>
    public interface IEventHandler : IDependency { }

    public interface IEventHandler<in T> : IEventHandler where T : IEvent {
        void Handle(T e);
    }
}