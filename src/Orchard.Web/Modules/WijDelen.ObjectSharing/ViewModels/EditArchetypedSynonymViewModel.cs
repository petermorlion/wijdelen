using System.Collections.Generic;
using System.Linq;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.ViewModels {
    public class EditArchetypedSynonymViewModel {
        public ArchetypedSynonymRecord Record { get; }

        public IList<ArchetypeRecord> Archetypes { get; }

        public ArchetypeRecord SelectedArchetype { get; set; }

        public EditArchetypedSynonymViewModel(ArchetypedSynonymRecord record, IList<ArchetypeRecord> archetypes) {
            Record = record;
            Archetypes = archetypes;
            SelectedArchetype = archetypes.SingleOrDefault(x => x.Name == record.Archetype);
        }
    }
}