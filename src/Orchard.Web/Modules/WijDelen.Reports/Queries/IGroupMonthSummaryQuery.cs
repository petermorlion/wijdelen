using System.Collections.Generic;
using Orchard;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    public interface IGroupMonthSummaryQuery : IDependency {
        IEnumerable<GroupMonthSummaryViewModel> GetResults(int year, int month);
    }
}