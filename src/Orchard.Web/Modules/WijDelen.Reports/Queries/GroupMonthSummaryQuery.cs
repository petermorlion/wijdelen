using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using WijDelen.ObjectSharing.Models;
using WijDelen.Reports.ViewModels;
using WijDelen.UserImport.Services;

namespace WijDelen.Reports.Queries {
    public class GroupMonthSummaryQuery : IGroupMonthSummaryQuery {
        private readonly IGroupsQuery _groupsQuery;
        private readonly IRepository<ObjectRequestRecord> _requestRepository;

        public GroupMonthSummaryQuery(IGroupsQuery groupsQuery, IRepository<ObjectRequestRecord> requestRepository) {
            _groupsQuery = groupsQuery;
            _requestRepository = requestRepository;
        }
        
        public IEnumerable<GroupMonthSummaryViewModel> GetResults(int year, int month) {
            var results = new List<GroupMonthSummaryViewModel>();

            var groups = _groupsQuery.GetResults();
            var requests = _requestRepository
                .Fetch(x => x.CreatedDateTime.Year == year && x.CreatedDateTime.Month == month)
                .GroupBy(x => x.GroupId);

            foreach (var groupRequests in requests) {
                var group = groups.SingleOrDefault(x => x.Id == groupRequests.Key);
                if (group == null) {
                    continue;
                }

                results.Add(new GroupMonthSummaryViewModel {
                    GroupName = group.Name,
                    ObjectRequestCount = groupRequests.Count()
                });
            }

            return results;
        }
    }
}