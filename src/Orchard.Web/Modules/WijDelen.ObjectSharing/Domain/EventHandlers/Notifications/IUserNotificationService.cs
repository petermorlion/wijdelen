using System.Collections.Generic;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.Events;

namespace WijDelen.ObjectSharing.Domain.EventHandlers.Notifications {
    /// <summary>
    /// A service to notify users about events in the system. An implementation is technology/transport-specific (e.g. email, mobile notifications, ...).
    /// An implementation is responsible for checking if the user has opted in for the notification and for making sure any exceptions are handled.
    /// </summary>
    public interface IUserNotificationService {
        void Handle(IEnumerable<IUser> users, ObjectRequested e);
        void Handle(IEnumerable<IUser> users, ObjectRequestUnblocked e);
    }
}