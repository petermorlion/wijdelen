using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Users.Models;
using WijDelen.ObjectSharing.Models;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    public class RequestsDetailsQuery : IRequestsDetailsQuery {
        private readonly ShellSettings _shellSettings;
        private readonly ITransactionManager _transactionManager;

        public RequestsDetailsQuery(ITransactionManager transactionManager, ShellSettings shellSettings) {
            _transactionManager = transactionManager;
            _shellSettings = shellSettings;
        }

        public IEnumerable<RequestsDetailsViewModel> GetResults(int? groupId, DateTime startDate, DateTime stopDate) {
            var session = _transactionManager.GetSession();

            if (stopDate.TimeOfDay == new TimeSpan(0, 0, 0)) stopDate = new DateTime(stopDate.Year, stopDate.Month, stopDate.Day, 23, 59, 59);

            var objectRequestRecord = _shellSettings.GetFullTableName(typeof(ObjectRequestRecord));
            var objectRequestMailRecord = _shellSettings.GetFullTableName(typeof(ObjectRequestMailRecord));
            var objectRequestResponseRecord = _shellSettings.GetFullTableName(typeof(ObjectRequestResponseRecord));
            var userPartRecord = _shellSettings.GetFullTableName(typeof(UserPartRecord));

            var requestsQuery = session.CreateSQLQuery("SELECT r.AggregateId, r.CreatedDateTime, u.Email, r.Description, COUNT(y.Response) AS 'YesCount', COUNT(n.Response) AS 'NoCount', COUNT(nn.Response) AS 'NotNowCount', r.GroupName " +
                                                       $"FROM {objectRequestRecord} r " +
                                                       $"    INNER JOIN {userPartRecord} u ON u.Id = r.UserId " +
                                                       $"    LEFT OUTER JOIN {objectRequestResponseRecord} y ON y.ObjectRequestId = r.AggregateId AND y.Response = 'Yes' " +
                                                       $"    LEFT OUTER JOIN {objectRequestResponseRecord} n ON n.ObjectRequestId = r.AggregateId AND n.Response = 'No' " +
                                                       $"    LEFT OUTER JOIN {objectRequestResponseRecord} nn ON nn.ObjectRequestId = r.AggregateId AND nn.Response = 'NotNow' " +
                                                       "WHERE r.CreatedDateTime >= :startDate AND r.CreatedDateTime <= :stopDate " +
                                                       (groupId.HasValue ? "AND r.GroupId = :groupId " : "") +
                                                       "GROUP BY r.AggregateId, r.CreatedDateTime, u.Email, r.Description, y.Response, n.Response, nn.Response, r.GroupName " +
                                                       "ORDER BY r.CreatedDateTime DESC")
                .SetParameter("startDate", startDate)
                .SetParameter("stopDate", stopDate);

            if (groupId.HasValue) {
                requestsQuery = requestsQuery.SetParameter("groupId", groupId.Value);
            }

            var requests = requestsQuery.List<object[]>();


            var mailsQuery = session.CreateSQLQuery("SELECT r.AggregateId, COUNT(m.ObjectRequestId) " +
                                                    $"FROM {objectRequestRecord} r " +
                                                    $"    INNER JOIN {objectRequestMailRecord} m ON m.ObjectRequestId = r.AggregateId " +
                                                    "WHERE r.CreatedDateTime >= :startDate AND r.CreatedDateTime <= :stopDate " +
                                                    (groupId.HasValue ? "AND r.GroupId = :groupId " : "") +
                                                    "GROUP BY r.AggregateId")
                .SetParameter("startDate", startDate)
                .SetParameter("stopDate", stopDate);

            if (groupId.HasValue) {
                mailsQuery = mailsQuery.SetParameter("groupId", groupId.Value);
            }

            var mails = mailsQuery.List<object[]>();

            var results = new List<RequestsDetailsViewModel>();

            foreach (var request in requests)
                results.Add(new RequestsDetailsViewModel {
                    CreatedDateTime = DateTime.Parse(request[1].ToString()),
                    Email = request[2].ToString(),
                    Description = request[3].ToString(),
                    MailCount = GetMailCountForId(request[0].ToString(), mails),
                    YesCount = int.Parse(request[4].ToString()),
                    NoCount = int.Parse(request[5].ToString()),
                    NotNowCount = int.Parse(request[6].ToString()),
                    GroupName = request[7].ToString()
                });

            return results;
        }

        private int GetMailCountForId(string id, IList<object[]> items)
        {
            var correctItem = items.SingleOrDefault(x => x[0].ToString() == id);
            if (correctItem == null) {
                return 0;
            }

            return (int)correctItem.ToList()[1];
        }
    }
}