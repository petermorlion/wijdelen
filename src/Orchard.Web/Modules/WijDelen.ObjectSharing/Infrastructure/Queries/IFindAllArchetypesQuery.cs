using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public interface IFindAllArchetypesQuery : IDependency {
        IEnumerable<ContentItem> GetResult();
    }
}