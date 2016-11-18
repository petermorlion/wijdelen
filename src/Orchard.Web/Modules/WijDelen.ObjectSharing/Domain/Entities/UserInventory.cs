using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Entities {
    /// <summary>
    /// Represents a what the user owns and wants to allow others to lend.
    /// </summary>
    public class UserInventory : EventSourced {
        private readonly IList<int> _ownedArchetypeIds = new List<int>();
        private readonly IList<int> _notOwnedArchetypeIds = new List<int>();

        private UserInventory(Guid id) : base(id) {
            Handles<UserInventoryCreated>(OnUserInventoryCreated);
            Handles<ArchetypeMarkedAsOwned>(OnArchetypeMarkedAsOwned);
            Handles<ArchetypeMarkedAsNotOwned>(OnArchetypeMarkedAsNotOwned);
        }

        public UserInventory(Guid id, int userId) : this(id) {
            Update(new UserInventoryCreated { UserId = userId });
        }

        public UserInventory(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        public void MarkAsOwned(int archetypeId, string archetypeTitle) {
            Update(new ArchetypeMarkedAsOwned { UserId = UserId, ArchetypeId = archetypeId, ArchetypeTitle = archetypeTitle});
        }

        public void MarkAsNotOwned(int archetypeId, string archetypeTitle) {
            Update(new ArchetypeMarkedAsNotOwned { UserId = UserId, ArchetypeId = archetypeId, ArchetypeTitle = archetypeTitle });
        }

        private void OnArchetypeMarkedAsOwned(ArchetypeMarkedAsOwned archetypeMarkedAsOwned) {
            _ownedArchetypeIds.Add(archetypeMarkedAsOwned.ArchetypeId);
            _notOwnedArchetypeIds.Remove(archetypeMarkedAsOwned.ArchetypeId);
        }

        private void OnArchetypeMarkedAsNotOwned(ArchetypeMarkedAsNotOwned archetypeMarkedAsNotOwned) {
            _notOwnedArchetypeIds.Add(archetypeMarkedAsNotOwned.ArchetypeId);
            _ownedArchetypeIds.Remove(archetypeMarkedAsNotOwned.ArchetypeId);
        }

        private void OnUserInventoryCreated(UserInventoryCreated objectRequested) {
            UserId = objectRequested.UserId;
        }

        public int UserId { get; private set; }

        /// <summary>
        /// The ids of the archetypes this user owns. These are the ids of the Orchard content items.
        /// </summary>
        public IEnumerable<int> OwnedArchetypeIds => _ownedArchetypeIds;

        /// <summary>
        /// The ids of the archetypes this user has explicitly marked as not owned. These are the ids of the Orchard content items.
        /// </summary>
        public IEnumerable<int> NotOwnedArchetypeIds => _notOwnedArchetypeIds;
    }
}