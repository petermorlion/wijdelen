using System;
using System.Linq;
using Orchard.ContentManagement;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Domain.Services;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class UserNotifier :
        IEventHandler<ObjectRequested> {
        private readonly IFindOtherUsersInGroupThatPossiblyOwnObjectQuery _userQuery;
        private readonly IRandomSampleService _randomSampleService;
        private readonly IEventSourcedRepository<ObjectRequestedNotification> _repository;

        public UserNotifier(IFindOtherUsersInGroupThatPossiblyOwnObjectQuery userQuery, IRandomSampleService randomSampleService, IEventSourcedRepository<ObjectRequestedNotification> repository) {
            _userQuery = userQuery;
            _randomSampleService = randomSampleService;
            _repository = repository;
        }

        public void Handle(ObjectRequested e) {
            var otherUsers = _userQuery.GetResults(e.UserId, e.Description).ToList();
            var approvedUsers = otherUsers.Where(x => x.As<GroupMembershipPart>().GroupMembershipStatus == GroupMembershipStatus.Approved).ToList();
            var randomUsers = _randomSampleService.GetRandomSample(approvedUsers, 50);

            var correlationId = Guid.NewGuid();

            foreach (var user in randomUsers) {
                var notification = new ObjectRequestedNotification(Guid.NewGuid(), e.UserId, user.Id, e.Description, e.ExtraInfo, e.SourceId);
                notification.Send();
                _repository.Save(notification, correlationId.ToString());
            }
        }
    }
}