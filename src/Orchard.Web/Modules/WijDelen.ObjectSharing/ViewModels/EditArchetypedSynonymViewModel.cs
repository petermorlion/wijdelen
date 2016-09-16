using System.Collections.Generic;

namespace WijDelen.ObjectSharing.ViewModels {
    public class EditArchetypedSynonymViewModel {
        public string Synonym { get; set; }

        public IList<ArchetypeViewModel> Archetypes { get; set; }

        public string SelectedArchetypeId { get; set; }
    }
}