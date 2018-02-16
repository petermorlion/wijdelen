using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.ValueTypes;

namespace WijDelen.ObjectSharing.Domain.Entities {
    /// <summary>
    /// Represents the notification 
    /// </summary>
    public class ObjectRequestedNotification : EventSourced {
        private ObjectRequestedNotification(Guid id) : base(id) {
            Handles<ObjectRequestedNotificationCreated>(OnObjectRequestedNotificationCreated);
            Handles<SendObjectRequestedNotificationRequested>(OnSendObjectRequestedNotificationRequested);
            Handles<ObjectRequestedNotificationSent>(OnObjectRequestedNotificationSent);
        }

        public ObjectRequestedNotification(Guid id, int requestingUserId, int receivingUserId, string description, string extraInfo, ObjectRequestStatus status, Guid objectRequestId) : this(id)
        {
            Update(new ObjectRequestedNotificationCreated {RequestingUserId = requestingUserId, ReceivingUserId = receivingUserId, Description = description, ExtraInfo = extraInfo, Status = status, ObjectRequestId = objectRequestId });
        }

        public ObjectRequestedNotification(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        public void Send() {
            Update(new SendObjectRequestedNotificationRequested { RequestingUserId = RequestingUserId, ReceivingUserId = ReceivingUserId, Description = Description, ExtraInfo = ExtraInfo, ObjectRequestId = ObjectRequestId, Status = Status });
        }

        public void MarkAsSent() {
            Update(new ObjectRequestedNotificationSent());
        }

        private void OnObjectRequestedNotificationCreated(ObjectRequestedNotificationCreated objectRequestedNotificationCreated) {
            ReceivingUserId = objectRequestedNotificationCreated.ReceivingUserId;
            RequestingUserId = objectRequestedNotificationCreated.RequestingUserId;
            Description = objectRequestedNotificationCreated.Description;
            ExtraInfo = objectRequestedNotificationCreated.ExtraInfo;
            ObjectRequestId = objectRequestedNotificationCreated.ObjectRequestId;
            Status = objectRequestedNotificationCreated.Status;
        }

        private void OnSendObjectRequestedNotificationRequested(SendObjectRequestedNotificationRequested sendObjectRequestedNotificationRequested) {
        }

        private void OnObjectRequestedNotificationSent(ObjectRequestedNotificationSent objectRequestedNotificationSent) {
        }

        /// <summary>
        /// The id of the user that requested the object to start this mail campaign.
        /// </summary>
        public int RequestingUserId { get; private set; }

        /// <summary>
        /// The id of the user that is receiving this notification.
        /// </summary>
        public int ReceivingUserId { get; private set; }

        /// <summary>
        /// A short description of the object
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Extra info as to why the user needs it, what he/she plans to do with it, etc.
        /// </summary>
        public string ExtraInfo { get; private set; }

        /// <summary>
        /// The status of the object request.
        /// </summary>
        public ObjectRequestStatus Status { get; set; }

        public Guid ObjectRequestId { get; private set; }
    }
}