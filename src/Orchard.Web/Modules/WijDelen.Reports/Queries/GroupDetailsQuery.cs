using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.ObjectSharing.Models;
using WijDelen.Reports.ViewModels;
using WijDelen.UserImport.Services;

namespace WijDelen.Reports.Queries {
    public class GroupDetailsQuery : IGroupDetailsQuery {
        private readonly IGroupService _groupService;
        private readonly ITransactionManager _transactionManager;
        private readonly ShellSettings _shellSettings;

        public GroupDetailsQuery(IGroupService groupService, ITransactionManager transactionManager, ShellSettings shellSettings) {
            _groupService = groupService;
            _transactionManager = transactionManager;
            _shellSettings = shellSettings;
        }

        private string GetFullTableName(Type type) {
            var tablePrefix = _shellSettings.DataTablePrefix;
            var featurePrefix = "WijDelen_ObjectSharing";
            if (string.IsNullOrWhiteSpace(tablePrefix)) {
                return $"{featurePrefix}_{type.Name}";
            }

            return $"{tablePrefix}_{featurePrefix}_{type.Name}";
        }

        public IEnumerable<GroupDetailsViewModel> GetResults(DateTime startDate, DateTime stopDate) {
            var results = new List<GroupDetailsViewModel>();

            var groups = _groupService.GetGroups();

            var session = _transactionManager.GetSession();

            if (stopDate.TimeOfDay == new TimeSpan(0, 0, 0)) {
                stopDate = new DateTime(stopDate.Year, stopDate.Month, stopDate.Day, 23, 59, 59);
            }

            var objectRequestRecord = GetFullTableName(typeof(ObjectRequestRecord));
            var objectRequestMailRecord = GetFullTableName(typeof(ObjectRequestMailRecord));
            var objectRequestResponseRecord = GetFullTableName(typeof(ObjectRequestResponseRecord));

            var requestsQuery = session.CreateSQLQuery("SELECT r.GroupId, COUNT(r.AggregateId) " +
                                               $"FROM {objectRequestRecord} r " +
                                               "WHERE r.CreatedDateTime >= :startDate AND r.CreatedDateTime <= :stopDate " +
                                               "GROUP BY r.GroupId")
                                .SetParameter("startDate", startDate)
                                .SetParameter("stopDate", stopDate);

            var requests = requestsQuery.List<object[]>();

            var mailsQuery = session.CreateSQLQuery("SELECT r.GroupId, COUNT(m.EmailAddress) " +
                                                    $"FROM {objectRequestRecord} r " +
                                                    $"    INNER JOIN {objectRequestMailRecord} m " +
                                                    "        ON r.AggregateId = m.ObjectRequestId " +
                                                    "WHERE m.SentDateTime >= :startDate " +
                                                    "    AND m.SentDateTime <= :stopDate " +
                                                    "GROUP BY r.GroupId")
                                .SetParameter("startDate", startDate)
                                .SetParameter("stopDate", stopDate);

            var mails = mailsQuery.List<object[]>();

            var responsesQuery = session.CreateSQLQuery("SELECT r.GroupId, re.Response, COUNT(re.DateTimeResponded) " +
                                                        $"FROM {objectRequestRecord} r " +
                                                        $"    INNER JOIN {objectRequestResponseRecord} re " +
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