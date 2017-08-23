using System;
using System.Collections.Generic;
using Orchard;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    /// <summary>
    /// Gets a report between two dates, for a given group. If null is given for the groupId,
    /// the report is returned for all groups.
    /// </summary>
    public interface IGroupDetailsQuery : IDependency {
        IEnumerable<GroupDetailsViewModel> GetResults(int? groupId, DateTime startDate, DateTime stopDate);
    }
}