using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public interface ISearchArchetypesByTitleQuery : IDependency {
        /// <summary>
        /// Returns al "Archetype" content items where the Title contains the given search term, ignoring case.
        /// </summary>
        /// <param name="title">The (part of) the title to search for.</param>
        /// <returns>An IEnumerable of content items</returns>
        IEnumerable<ContentItem> GetResult(string title);
    }
}