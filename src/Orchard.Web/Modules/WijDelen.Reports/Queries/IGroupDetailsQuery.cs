using System;
using System.Collections.Generic;
using Orchard;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    public interface IGroupDetailsQuery : IDependency {
        IEnumerable<GroupDetailsViewModel> GetResults(DateTime startDate, DateTime stopDate);
    }
}