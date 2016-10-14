using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public class FindSynonymsByExactMatchQuery : IFindSynonymsByExactMatchQuery {
        private readonly IContentManager _contentManager;

        public FindSynonymsByExactMatchQuery(IContentManager contentManager) {
            _contentManager = contentManager;
        }
        public IEnumerable<ContentItem> GetResults(string title) {
            var synonyms = _contentManager
                .Query(VersionOptions.Published, "Synonym")
                .Join<TitlePartRecord>()
                .List();

            var synonymMatches = synonyms
                .Where(x => x.As<TitlePart>().Title.ToLower() == title.ToLowerInvariant())
                .ToList();

            return synonymMatches;
        }
    }
}