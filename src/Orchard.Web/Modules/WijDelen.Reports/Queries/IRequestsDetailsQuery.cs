using System;
using System.Collections.Generic;
using Orchard;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    /// <summary>
    /// Gets a detailed report on the requests for a group between two dates.
    /// </summary>
    public interface IRequestsDetailsQuery : IDependency {
        IEnumerable<RequestsDetailsViewModel> GetResults(int? groupId, DateTime startDate, DateTime stopDate);
    }
}