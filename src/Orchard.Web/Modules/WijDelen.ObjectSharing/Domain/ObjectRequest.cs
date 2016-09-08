using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain {
    /// <summary>
    /// Represents a request for an object by a user.
    /// </summary>
    public class ObjectRequest : EventSourced
    {
        public ObjectRequest(int id) : base(id) {
            Handles<ObjectRequestInfoUpdated>(OnInfoUpdated);
        }

        private void OnInfoUpdated(ObjectRequestInfoUpdated infoUpdated) {
            Description = infoUpdated.Description;
            ExtraInfo = infoUpdated.ExtraInfo;
        }

        /// <summary>
        /// Sets the (shorter) description and the (longer) extra info.
        /// </summary>
        /// <param name="description">A short description of the object</param>
        /// <param name="extraInfo">Extra info as to why the user needs it, what he/she plans to do with it, etc.</param>
        public void UpdateInfo(string description, string extraInfo) {
            Update(new ObjectRequestInfoUpdated { Description = description, ExtraInfo = extraInfo });
        }

        /// <summary>
        /// A short description of the object
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Extra info as to why the user needs it, what he/she plans to do with it, etc.
        /// </summary>
        public string ExtraInfo { get; private set; }
    }
}