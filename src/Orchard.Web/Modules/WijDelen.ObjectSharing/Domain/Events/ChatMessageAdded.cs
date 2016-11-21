using System;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Events {
    public class ChatMessageAdded : VersionedEvent
    {
        public DateTime DateTime { get; set; }
        public Guid ChatId { get; set; }

        public int UserId { get; set; }

        public string Message { get; set; }
    }
}