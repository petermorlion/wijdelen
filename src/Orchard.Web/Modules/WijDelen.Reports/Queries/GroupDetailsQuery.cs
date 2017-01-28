using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.Reports.ViewModels;
using WijDelen.UserImport.Services;

namespace WijDelen.Reports.Queries {
    public class GroupDetailsQuery : IGroupDetailsQuery {
        private readonly IGroupService _groupService;
        private readonly ITransactionManager _transactionManager;

        public GroupDetailsQuery(IGroupService groupService, ITransactionManager transactionManager) {
            _groupService = groupService;
            _transactionManager = transactionManager;
        }

        public IEnumerable<GroupDetailsViewModel> GetResults(DateTime startDate, DateTime stopDate) {
            var results = new List<GroupDetailsViewModel>();

            var groups = _groupService.GetGroups();

            var session = _transactionManager.GetSession();

            if (stopDate.TimeOfDay == new TimeSpan(0, 0, 0)) {
                stopDate = new DateTime(stopDate.Year, stopDate.Month, stopDate.Day, 23, 59, 59);
            }

            // TODO: table names
            var requestsQuery = session.CreateSQLQuery("SELECT r.GroupId, COUNT(r.AggregateId) " +
                                               "FROM WijDelen_ObjectSharing_ObjectRequestRecord r " +
                                               "WHERE r.CreatedDateTime >= :startDate AND r.CreatedDateTime <= :stopDate " +
                                               "GROUP BY r.GroupId")
                                .SetParameter("startDate", startDate)
                                .SetParameter("stopDate", stopDate);

            var requests = requestsQuery.List<object[]>();

            var mailsQuery = session.CreateSQLQuery("SELECT r.GroupId, COUNT(m.EmailAddress) " +
                                                    "FROM WijDelen_ObjectSharing_ObjectRequestRecord r " +
                                                    "    INNER JOIN WijDelen_ObjectSharing_ObjectRequestMailRecord m " +
                                                    "        ON r.AggregateId = m.ObjectRequestId " +
                                                    "WHERE m.SentDateTime >= :startDate " +
                                                    "    AND m.SentDateTime <= :stopDate " +
                                                    "GROUP BY r.GroupId")
                                .SetParameter("startDate", startDate)
                                .SetParameter("stopDate", stopDate);

            var mails = mailsQuery.List<object[]>();

            var responsesQuery = session.CreateSQLQuery("SELECT r.GroupId, re.Response, COUNT(re.DateTimeResponded) " +
                                                        "FROM WijDelen_ObjectSharing_ObjectRequestRecord r " +
                                                        "    INNER JOIN WijDelen_ObjectSharing_ObjectRequestResponseRecord re " +
                                                        "        ON r.AggregateId = re.ObjectRequestId " +
                                                        "WHERE re.DateTimeResponded >= :startDate " +
                                                        "    AND re.DateTimeResponded <= :stopDate " +
                                                        "GROUP BY r.GroupId, re.Response")
                                .SetParameter("startDate", startDate)
                                .SetParameter("stopDate", stopDate);

            var responses = responsesQuery.List<object[]>();

            foreach (var groupViewModel in groups) {
                results.Add(new GroupDetailsViewModel {
                    GroupName = groupViewModel.Name,
                    RequestCount = GetCountForId(groupViewModel.Id, requests),
                    MailCount = GetCountForId(groupViewModel.Id, mails),
                    YesCount = GetResponseCountForId(groupViewModel.Id, responses, ObjectRequestAnswer.Yes),
                    NoCount = GetResponseCountForId(groupViewModel.Id, responses, ObjectRequestAnswer.No),
                    NotNowCount = GetResponseCountForId(groupViewModel.Id, responses, ObjectRequestAnswer.NotNow)
                });
            }

            return results;
        }

        private int GetCountForId(int id, IList<object[]> items) {
            var list = items.ToList();
            var correctItem = list.SingleOrDefault(x => (int)x.ToList()[0] == id);
            if (correctItem == null) {
                return 0;
            }

            return (int)correctItem.ToList()[1];
        }

        private int GetResponseCountForId(int id, IList<object[]> items, ObjectRequestAnswer answer) {
            var list = items.ToList();
            var correctItem = list.SingleOrDefault(x => (int)x.ToList()[0] == id && x.ToList()[1].ToString() == answer.ToString());
            if (correctItem == null) {
                return 0;
            }

            return (int)correctItem.ToList()[2];
        }
    }
}