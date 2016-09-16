using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Entities {
    /// <summary>
    /// The archetype for an item that can be shared or requested, containing any synonyms because users might enter
    /// different descriptions for the same item.
    /// </summary>
    public class ItemArchetype : EventSourced {
        private ItemArchetype(Guid id) : base(id) {
            Handles<ItemArchetypeCreated>(OnCreated);
        }

        private void OnCreated(ItemArchetypeCreated e) {
            Name = e.Name;
        }

        public ItemArchetype(Guid id, string name) : this(id) {
            Update(new ItemArchetypeCreated { Name = name });
        }

        public ItemArchetype(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        public string Name { get; private set; }
    }
}