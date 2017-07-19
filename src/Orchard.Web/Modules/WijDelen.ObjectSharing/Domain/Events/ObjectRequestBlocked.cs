using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ObjectRequestBlocked : VersionedEvent {
        /// <summary>
        /// A short description of the object
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Extra info as to why the user needs it, what he/she plans to do with it, etc.
        /// </summary>
        public string ExtraInfo { get; set; }

        /// <summary>
        /// The id of the user that requested the object.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The words used that make this request forbidden.
        /// </summary>
        public IList<string> ForbiddenWords { get; set; }

        /// <summary>
        /// The reason the request was blocked, when blocked manually.
        /// </summary>
        public string Reason { get; set; }
    }
}