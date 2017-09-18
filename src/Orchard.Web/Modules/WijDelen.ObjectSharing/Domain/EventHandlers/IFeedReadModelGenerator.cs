using Orchard;
using WijDelen.ObjectSharing.Domain.Events;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public interface IFeedReadModelGenerator : IDependency {
        void Handle(ChatMessageAdded e);
        void Handle(ObjectRequested e);
        void Handle(ObjectRequestConfirmed e);
    }
}