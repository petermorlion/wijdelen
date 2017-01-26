using Orchard;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    public interface IMonthSummaryQuery : IDependency {
        SummaryViewModel GetResults(int monthNumber);
    }
}