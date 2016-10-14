using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public interface IFindSynonymsByExactMatchQuery : IDependency {
        IEnumerable<ContentItem> GetResults(string title);
    }
}