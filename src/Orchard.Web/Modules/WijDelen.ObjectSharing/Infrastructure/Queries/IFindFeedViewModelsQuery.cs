using System.Collections.Generic;
using Orchard;
using WijDelen.ObjectSharing.ViewModels.Feed;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public interface IFindFeedViewModelsQuery : IDependency {
        IList<IFeedItemViewModel> GetResults(int groupId, int userId, int take);
    }
}