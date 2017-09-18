using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Environment.Configuration;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels.Feed;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public class FindFeedViewModelsQuery : IFindFeedViewModelsQuery {
        private readonly IFindUsersByIdsQuery _findUsersByIdsQuery;
        private readonly IRepository<ObjectRequestResponseRecord> _objectRequestResponseRepository;
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly ShellSettings _shellSettings;
        private readonly ITransactionManager _transactionManager;

        public FindFeedViewModelsQuery(
            IRepository<ObjectRequestRecord> objectRequestRepository,
            ITransactionManager transactionManager,
            ShellSettings shellSettings,
            IFindUsersByIdsQuery findUsersByIdsQuery,
            IRepository<ObjectRequestResponseRecord> objectRequestResponseRepository) {
            _objectRequestRepository = objectRequestRepository;
            _transactionManager = transactionManager;
            _shellSettings = shellSettings;
            _findUsersByIdsQuery = findUsersByIdsQuery;
            _objectRequestResponseRepository = objectRequestResponseRepository;
        }

        public IList<IFeedItemViewModel> GetResults(int groupId, int userId, int take) {
            var results = new List<IFeedItemViewModel>();

            var objectRequests = GetObjectRequests(groupId, userId, take);
            results.AddRange(objectRequests);

            var chatMessages = GetChatMessages(userId, take);
            results.AddRange(chatMessages);

            results = results.OrderByDescending(x => x.DateTime).Take(take).ToList();

            return results;
        }

        private IEnumerable<ObjectRequestViewModel> GetObjectRequests(int groupId, int userId, int take) {
            var objectRequests = _objectRequestRepository
                .Fetch(x => x.GroupId == groupId && x.UserId != userId && x.Status == ObjectRequestStatus.None.ToString())
                .OrderByDescending(x => x.CreatedDateTime)
                .Take(take)
                .ToList();

            var objectRequestIds = objectRequests.Select(x => x.AggregateId).ToList();
            var objectRequestResponses = _objectRequestResponseRepository.Fetch(x => objectRequestIds.Contains(x.ObjectRequestId) && x.UserId == userId).ToList();

            var userIds = objectRequests.Select(x => x.UserId).Distinct().ToList();
            var users = _findUsersByIdsQuery.GetResult(userIds.ToArray()).ToList();

            var viewModels = new List<ObjectRequestViewModel>();
            foreach (var objectRequestRecord in objectRequests) {
                var user = users.SingleOrDefault(x => x.Id == objectRequestRecord.UserId);
                var answer = objectRequestResponses.SingleOrDefault(x => x.ObjectRequestId == objectRequestRecord.AggregateId)?.Response;

                viewModels.Add(new ObjectRequestViewModel {
                    DateTime = objectRequestRecord.CreatedDateTime.ToLocalTime(),
                    Description = objectRequestRecord.Description,
                    UserName = user?.GetUserDisplayName(),
                    ChatCount = objectRequestRecord.ChatCount,
                    ExtraInfo = objectRequestRecord.ExtraInfo,
                    ObjectRequestId = objectRequestRecord.AggregateId,
                    CurrentUsersResponse = answer
                });
            }

            return viewModels;
        }

        private IEnumerable<ChatMessageViewModel> GetChatMessages(int userId, int take) {
            var session = _transactionManager.GetSession();
            var objectRequestRecord = _shellSettings.GetFullTableName(typeof(ObjectRequestRecord));
            var chatRecord = _shellSettings.GetFullTableName(typeof(ChatRecord));
            var chatMessageRecord = _shellSettings.GetFullTableName(typeof(ChatMessageRecord));

            var chatQuery = session.CreateSQLQuery($"SELECT TOP {take} c.ChatId, o.Description, cm.DateTime, cm.UserName " +
                                                   $"FROM {chatRecord} c " +
                                                   $"INNER JOIN {chatMessageRecord} cm ON cm.ChatId = c.ChatId " +
                                                   $"INNER JOIN {objectRequestRecord} o ON o.AggregateId = c.ObjectRequestId " +
                                                   "WHERE (c.RequestingUserId = :userId OR c.ConfirmingUserId = :userId) AND cm.UserId != :userId " +
                                                   "ORDER BY cm.DateTime DESC")
                .SetParameter("userId", userId);

            var records = chatQuery.List<object[]>();
            return records.Select(x => new ChatMessageViewModel {
                ChatId = Guid.Parse(x[0].ToString()),
                Description = x[1].ToString(),
                DateTime = DateTime.Parse(x[2].ToString()).ToLocalTime(),
                UserName = x[3].ToString()
            });
        }
    }
}