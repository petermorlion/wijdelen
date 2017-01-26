using Orchard;
using WijDelen.Reports.Models;

namespace WijDelen.Reports.Queries {
    public interface ITotalsQuery : IDependency {
        Totals GetResults();
    }
}