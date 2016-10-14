using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using System.Linq;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public class FindArchetypesByTitleQuery : IFindArchetypesByTitleQuery {
        private readonly IContentManager _contentManager;

        public FindArchetypesByTitleQuery(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<ContentItem> GetResult(string title) {
            var archetypes = _contentManager
                .Query(VersionOptions.Published, "Archetype")
                .Join<TitlePartRecord>()
                .List();

            var archetypeMatches = archetypes
                .Where(x => x.As<TitlePart>().Title.ToLower().Contains(title.ToLowerInvariant()))
                .ToList();

            return archetypeMatches;
        }
    }
}