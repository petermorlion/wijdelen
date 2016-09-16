using System.Collections.Generic;
using System.Linq;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.ViewModels {
    public class SynonymsViewModel {
        public SynonymsViewModel(IEnumerable<ArchetypedSynonymRecord> unarchetypedSynonyms, IEnumerable<ArchetypeRecord> archetypes) {
            var orderedArchetypes = archetypes.OrderBy(x => x.Name).ToList();
            Synonyms = unarchetypedSynonyms.OrderBy(x => x.Synonym).Select(x => new EditArchetypedSynonymViewModel(x, orderedArchetypes)).ToList();
        }

        public IList<EditArchetypedSynonymViewModel> Synonyms { get; }
    }
}