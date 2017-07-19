using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ObjectRequestBlockedByAdmin : VersionedEvent
    {
        /// <summary>
        /// The user that made the request.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The reason the request was blocked, when blocked manually.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The description of the request.
        /// </summary>
        public string Description { get; set; }
    }
}