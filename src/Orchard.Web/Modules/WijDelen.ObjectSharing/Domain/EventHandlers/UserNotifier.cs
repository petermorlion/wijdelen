using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Domain.EventHandlers.Notifications;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Domain.Services;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.UserImport.Models;
using IMailService = WijDelen.ObjectSharing.Infrastructure.IMailService;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class UserNotifier : IEventHandler<ObjectRequested>, IEventHandler<ObjectRequestUnblocked>, IEventHandler<ObjectRequestBlocked>, IEventHandler<ObjectRequestBlockedByAdmin> {
        private readonly IFindOtherUsersInGroupThatPossiblyOwnObjectQuery _findOtherUsersQuery;
        private readonly IGetUserByIdQuery _getUserByIdQuery;
        private readonly IMailService _mailService;
        private readonly IOrchardServices _orchardServices;
        private readonly IRandomSampleService _randomSampleService;
        private readonly IEnumerable<IUserNotificationService> _userNotificationServices;

        public UserNotifier(
            IMailService mailService,
            IGetUserByIdQuery getUserByIdQuery,
            IRandomSampleService randomSampleService,
            IFindOtherUsersInGroupThatPossiblyOwnObjectQuery findOtherUsersQuery,
            IOrchardServices orchardServices,
            IEnumerable<IUserNotificationService> userNotificationServices) {
            _mailService = mailService;
            _getUserByIdQuery = getUserByIdQuery;
            _randomSampleService = randomSampleService;
            _findOtherUsersQuery = findOtherUsersQuery;
            _orchardServices = orchardServices;
            _userNotificationServices = userNotificationServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Handle(ObjectRequestBlocked e) {
            var requestingUser = _getUserByIdQuery.GetResult(e.UserId);
            _mailService.SendAdminObjectRequestBlockedMail(e.SourceId, requestingUser.GetUserDisplayName(), e.Description, e.ExtraInfo, e.ForbiddenWords);
        }

        public void Handle(ObjectRequestBlockedByAdmin e) {
            var requestingUser = _getUserByIdQuery.GetResult(e.UserId);
            _mailService.SendObjectRequestBlockedMail(requestingUser, e.SourceId, e.Description, e.Reason);
        }

        public void Handle(ObjectRequested e) {
            if (e.Status == ObjectRequestStatus.BlockedForForbiddenWords) {
                _orchardServices.Notifier.Add(NotifyType.Warning, T("Thank you for your request. We noticed some words that might be considered offensive. If our system flagged this incorrectly, we will send your request to the members of your group."));
                return;
            }

            var otherUsers = _findOtherUsersQuery.GetResults(e.UserId, e.Description).ToList();
            var approvedUsers = otherUsers.Where(x => x.As<GroupMembershipPart>().GroupMembershipStatus == GroupMembershipStatus.Approved).ToList();
            var randomUsers = _randomSampleService.GetRandomSample(approvedUsers, 50);

            foreach (var notificationService in _userNotificationServices) notificationService.Handle(randomUsers, e);

            _orchardServices.Notifier.Add(NotifyType.Success, T("Thank you for your request. We sent your request to the members of your group."));

            var requestingUser = _getUserByIdQuery.GetResult(e.UserId);
            _mailService.SendAdminObjectRequestMail(requestingUser.GetUserDisplayName(), e.Description, e.ExtraInfo);
        }

        public void Handle(ObjectRequestUnblocked e) {
            if (e.WasPreviouslyBlockedByAdmin) return;

            var otherUsers = _findOtherUsersQuery.GetResults(e.UserId, e.Description).ToList();
            var approvedUsers = otherUsers.Where(x => x.As<GroupMembershipPart>().GroupMembershipStatus == GroupMembershipStatus.Approved).ToList();
            var randomUsers = _randomSampleService.GetRandomSample(approvedUsers, 50);

            foreach (var notificationService in _userNotificationServices)
                notificationService.Handle(randomUsers, e);
        }
    }
}