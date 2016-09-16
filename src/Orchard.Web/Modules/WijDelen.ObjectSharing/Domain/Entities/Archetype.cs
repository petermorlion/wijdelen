using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;

namespace WijDelen.ObjectSharing.Domain.Entities {
    /// <summary>
    /// The archetype for an item that can be shared or requested, containing any synonyms because users might enter
    /// different descriptions for the same item.
    /// </summary>
    public class Archetype : EventSourced {
        private readonly IList<string> _synonyms = new List<string>();

        private Archetype(Guid id) : base(id) {
            Handles<ArchetypeCreated>(OnCreated);
            Handles<ArchetypeSynonymAdded>(OnSynonymAdded);
        }

        private void OnCreated(ArchetypeCreated e) {
            Name = e.Name;
        }

        private void OnSynonymAdded(ArchetypeSynonymAdded e) {
            if (!_synonyms.Contains(e.Synonym)) {
                _synonyms.Add(e.Synonym);
            }
        }

        public Archetype(Guid id, string name) : this(id) {
            Update(new ArchetypeCreated { Name = name });
        }

        public Archetype(Guid id, IEnumerable<IVersionedEvent> history) : this(id) {
            LoadFrom(history);
        }

        public string Name { get; private set; }
        public IEnumerable<string> Synonyms => _synonyms;

        public void AddSynonym(string synonym) {
            Update(new ArchetypeSynonymAdded { Synonym = synonym });
        }
    }
}