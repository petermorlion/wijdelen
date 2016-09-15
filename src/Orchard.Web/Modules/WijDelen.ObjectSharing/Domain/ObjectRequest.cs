using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain {
    /// <summary>
    /// Represents a request for an object by a user.
    /// </summary>
    public class ObjectRequest : EventSourced
    {
        public ObjectRequest(Guid id) : base(id) {
            Handles<ObjectRequested>(OnObjectRequested);
        }

        public ObjectRequest(Guid id, string description, string extraInfo, int userId) : this(id) {
            Update(new ObjectRequested { Description = description, ExtraInfo = extraInfo, UserId = userId });
        }

        public ObjectRequest(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        private void OnObjectRequested(ObjectRequested objectRequested) {
            Description = objectRequested.Description;
            ExtraInfo = objectRequested.ExtraInfo;
            UserId = objectRequested.UserId;
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
    }
}