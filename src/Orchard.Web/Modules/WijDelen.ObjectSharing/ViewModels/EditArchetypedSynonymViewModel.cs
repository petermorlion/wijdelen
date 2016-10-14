using System.Collections.Generic;
using Orchard.ContentManagement;

namespace WijDelen.ObjectSharing.ViewModels {
    public class EditArchetypedSynonymViewModel {
        public string Synonym { get; set; }

        public IList<ContentItem> Archetypes { get; set; }

        public int? SelectedArchetypeId { get; set; }
    }
}