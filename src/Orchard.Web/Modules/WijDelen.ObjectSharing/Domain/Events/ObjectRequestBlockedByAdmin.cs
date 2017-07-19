using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ObjectRequestBlockedByAdmin : VersionedEvent
    {
        /// <summary>
        /// The reason the request was blocked, when blocked manually.
        /// </summary>
        public string Reason { get; set; }
    }
}