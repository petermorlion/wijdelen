using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public interface IFeedReadModelGenerator : 
        IEventHandler<ChatMessageAdded>,
        IEventHandler<ObjectRequested>,
        IEventHandler<ObjectRequestConfirmed>,
        IEventHandler<ObjectRequestDenied>,
        IEventHandler<ObjectRequestDeniedForNow>,
        IEventHandler<ChatStarted> {
    }
}