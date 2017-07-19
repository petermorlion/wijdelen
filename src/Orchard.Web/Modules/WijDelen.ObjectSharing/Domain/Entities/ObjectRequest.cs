using System;
using System.Collections.Generic;
using System.Linq;
using WijDelen.ObjectSharing.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.ValueTypes;

namespace WijDelen.ObjectSharing.Domain.Entities {
    /// <summary>
    /// Represents a request for an object by a user.
    /// </summary>
    public class ObjectRequest : EventSourced {
        private readonly IList<int> _confirmingUserIds = new List<int>();
        private readonly IList<int> _denyingUserIds = new List<int>();
        private readonly IList<int> _denyingForNowUserIds = new List<int>();

        private ObjectRequest(Guid id) : base(id) {
            Handles<ObjectRequested>(OnObjectRequested);
            Handles<ObjectRequestConfirmed>(OnObjectRequestConfirmed);
            Handles<ObjectRequestDenied>(OnObjectRequestDenied);
            Handles<ObjectRequestDeniedForNow>(OnObjectRequestDeniedForNow);
            Handles<ObjectRequestUnblocked>(OnObjectRequestUnblocked);
            Handles<ObjectRequestBlocked>(OnObjectRequestBlocked);
            Handles<ObjectRequestBlockedByAdmin>(OnObjectRequestBlockedByAdmin);
        }

        public ObjectRequest(Guid id, string description, string extraInfo, int userId) : this(id) {
            var status = ObjectRequestStatus.None;
            var forbiddenWords = ForbiddenWords.GetForbiddenWordsInString(description).Union(ForbiddenWords.GetForbiddenWordsInString(extraInfo)).ToList();
            if (forbiddenWords.Any()) {
                status = ObjectRequestStatus.BlockedForForbiddenWords;
            }

            Update(new ObjectRequested {Description = description, ExtraInfo = extraInfo, UserId = userId, CreatedDateTime = DateTime.UtcNow, Status = status});

            if (status == ObjectRequestStatus.BlockedForForbiddenWords) {
                Update(new ObjectRequestBlocked {Description = description, ExtraInfo = extraInfo, UserId = userId, ForbiddenWords = forbiddenWords});
            }
        }

        public ObjectRequest(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        public void Confirm(int confirmingUserId) {
            Update(new ObjectRequestConfirmed {ConfirmingUserId = confirmingUserId, DateTimeConfirmed = DateTime.UtcNow});
        }

        public void Deny(int denyingUserId) {
            Update(new ObjectRequestDenied {DenyingUserId = denyingUserId, DateTimeDenied = DateTime.UtcNow});
        }

        public void DenyForNow(int denyingUserId) {
            Update(new ObjectRequestDeniedForNow { DenyingUserId = denyingUserId, DateTimeDenied = DateTime.UtcNow });
        }

        public void Block(string reason) {
            Update(new ObjectRequestBlockedByAdmin { Reason = reason });
        }

        public void Unblock() {
            Update(new ObjectRequestUnblocked { Description = Description, ExtraInfo = ExtraInfo, UserId = UserId });
        }

        private void OnObjectRequested(ObjectRequested objectRequested) {
            Description = objectRequested.Description;
            ExtraInfo = objectRequested.ExtraInfo;
            UserId = objectRequested.UserId;
            CreatedDateTime = objectRequested.CreatedDateTime;
            Status = objectRequested.Status;
        }

        private void OnObjectRequestConfirmed(ObjectRequestConfirmed objectRequestConfirmed) {
            _confirmingUserIds.Add(objectRequestConfirmed.ConfirmingUserId);
        }

        private void OnObjectRequestDenied(ObjectRequestDenied objectRequestDenied) {
            _denyingUserIds.Add(objectRequestDenied.DenyingUserId);
        }

        private void OnObjectRequestDeniedForNow(ObjectRequestDeniedForNow objectRequestDeniedForNow) {
            _denyingForNowUserIds.Add(objectRequestDeniedForNow.DenyingUserId);
        }

        private void OnObjectRequestUnblocked(ObjectRequestUnblocked objectRequestUnblocked) {
            Status = ObjectRequestStatus.None;
        }

        private void OnObjectRequestBlocked(ObjectRequestBlocked objectRequestBlocked) {
        }

        private void OnObjectRequestBlockedByAdmin(ObjectRequestBlockedByAdmin objectRequestBlockedByAdmin) {
            Status = ObjectRequestStatus.BlockedByAdmin;
            BlockReason = objectRequestBlockedByAdmin.Reason;
        }

        /// <summary>
        /// A short description of the object
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Extra info as to why the user needs it, what he/she plans to do with it, etc.
        /// </summary>
        public string ExtraInfo { get; private set; }

        /// <summary>
        /// The id of the user that requested the object.
        /// </summary>
        public int UserId { get; private set; }

        /// <summary>
        /// The DateTime when the object request was made.
        /// </summary>
        public DateTime CreatedDateTime { get; private set; }

        public IEnumerable<int> ConfirmingUserIds => _confirmingUserIds;
        public IEnumerable<int> DenyingUserIds => _denyingUserIds;
        public IEnumerable<int> DenyingForNowUserIds => _denyingForNowUserIds;
        public ObjectRequestStatus Status { get; private set; }
        public string BlockReason { get; private set; }
    }
}