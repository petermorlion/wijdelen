using System.Collections.Generic;
using Orchard.ContentManagement;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public class FindAllSynonymsQuery : IFindAllSynonymsQuery {
        private readonly IContentManager _contentManager;

        public FindAllSynonymsQuery(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<ContentItem> GetResult() {
            var synonyms = _contentManager
                .Query("Synonym")
                .List();

            return synonyms;
        }
    }
}