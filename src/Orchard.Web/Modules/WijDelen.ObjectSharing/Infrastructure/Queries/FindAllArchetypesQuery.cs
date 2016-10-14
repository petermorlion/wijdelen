using System.Collections.Generic;
using Orchard.ContentManagement;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public class FindAllArchetypesQuery : IFindAllArchetypesQuery {
        private readonly IContentManager _contentManager;

        public FindAllArchetypesQuery(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<ContentItem> GetResult() {
            var archetypes = _contentManager
                .Query("Archetype")
                .List();

            return archetypes;
        }
    }
}