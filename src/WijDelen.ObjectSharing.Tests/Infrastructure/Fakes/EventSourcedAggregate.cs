using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Tests.Infrastructure.Fakes {
    public class EventSourcedAggregate : EventSourced {
        public EventSourcedAggregate(Guid id) : base(id) {
            Handles<Created>(OnCreated);
            Handles<Modified>(OnModified);
            Update(new Created());
        }

        public EventSourcedAggregate(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        public string Status { get; private set; }

        public void Modify() {
            Update(new Modified());
        }

        private void OnCreated(Created obj) {
            Status = "Created";
        }

        private void OnModified(Modified obj)
        {
            Status = "Modified";
        }
    }
}