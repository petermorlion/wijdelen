using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Entities {
    /// <summary>
    /// Represents a what the user owns and wants to allow others to lend.
    /// </summary>
    public class UserInventory : EventSourced
    {
        private UserInventory(Guid id) : base(id) {
            Handles<UserInventoryCreated>(OnUserInventoryCreated);
        }

        public UserInventory(Guid id, int userId) : this(id) {
            Update(new UserInventoryCreated { UserId = userId });
        }

        public UserInventory(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        private void OnUserInventoryCreated(UserInventoryCreated objectRequested) {
            UserId = objectRequested.UserId;
        }

        public int UserId { get; private set; }
    }
}