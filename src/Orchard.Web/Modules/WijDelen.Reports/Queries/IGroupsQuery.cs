using System.Collections.Generic;
using Orchard;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    public interface IGroupsQuery : IDependency {
        IEnumerable<GroupViewModel> GetResults();
    }
}