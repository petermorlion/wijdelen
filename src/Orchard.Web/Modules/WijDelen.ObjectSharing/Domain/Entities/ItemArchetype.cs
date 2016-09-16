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
        private readonly IList<string> _synonyms = new List<string>();

        private ItemArchetype(Guid id) : base(id) {
            Handles<ItemArchetypeCreated>(OnCreated);
            Handles<ItemArchetypeSynonymAdded>(OnSynonymAdded);
        }

        private void OnCreated(ItemArchetypeCreated e) {
            Name = e.Name;
        }

        private void OnSynonymAdded(ItemArchetypeSynonymAdded e) {
            if (!_synonyms.Contains(e.Synonym)) {
                _synonyms.Add(e.Synonym);
            }
        }

        public ItemArchetype(Guid id, string name) : this(id) {
            Update(new ItemArchetypeCreated { Name = name });
        }

        public ItemArchetype(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        public string Name { get; private set; }
        public IEnumerable<string> Synonyms => _synonyms;

        public void AddSynonym(string synonym) {
            Update(new ItemArchetypeSynonymAdded { Synonym = synonym });
        }
    }
}