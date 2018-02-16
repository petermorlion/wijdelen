using System;
using System.Collections.Generic;
using Orchard.Data;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers.Notifications {
    /// <summary>
    /// Creates the necessary records in the database so the user has an overview of received notifications.
    /// </summary>
    public class DatabaseNotificationService : IUserNotificationService {
        private readonly IRepository<ReceivedObjectRequestRecord> _repository;

        public DatabaseNotificationService(IRepository<ReceivedObjectRequestRecord> repository) {
            _repository = repository;
        }

        public void Handle(IEnumerable<IUser> users, ObjectRequested e) {
            StoreRecord(users, e.SourceId, e.Description, e.ExtraInfo, e.CreatedDateTime, e.UserId);
        }

        public void Handle(IEnumerable<IUser> users, ObjectRequestUnblocked e) {
            StoreRecord(users, e.SourceId, e.Description, e.ExtraInfo, DateTime.UtcNow, e.UserId);
        }

        private void StoreRecord(IEnumerable<IUser> users, Guid objectRequestId, string description, string extraInfo, DateTime receivedDateTime, int requestingUserId) {
            foreach (var user in users)
                _repository.Create(new ReceivedObjectRequestRecord {
                    UserId = user.Id,
                    ObjectRequestId = objectRequestId,
                    Description = description,
                    ExtraInfo = extraInfo,
                    ReceivedDateTime = receivedDateTime,
                    RequestingUserId = requestingUserId
                });
        }
    }
}