using System;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Domain.Services;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using IMailService = WijDelen.ObjectSharing.Infrastructure.IMailService;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ObjectRequestMailer : IEventHandler<SendObjectRequestedNotificationRequested>, IEventHandler<ObjectRequestUnblocked>, IEventHandler<ObjectRequestBlocked>, IEventHandler<ObjectRequestBlockedByAdmin>
    {
        private readonly IEventSourcedRepository<ObjectRequestMail> _repository;
        private readonly IGroupService _groupService;
        private readonly IMailService _mailService;
        private readonly IGetUserByIdQuery _getUserByIdQuery;
        private readonly IRandomSampleService _randomSampleService;
        private readonly IFindOtherUsersInGroupThatPossiblyOwnObjectQuery _findOtherUsersQuery;
        private readonly IOrchardServices _orchardServices;

        public ObjectRequestMailer(
            IEventSourcedRepository<ObjectRequestMail> repository, 
            IGroupService groupService,
            IMailService mailService,
            IGetUserByIdQuery getUserByIdQuery,
            IRandomSampleService randomSampleService,
            IFindOtherUsersInGroupThatPossiblyOwnObjectQuery findOtherUsersQuery,
            IOrchardServices orchardServices) {
            _repository = repository;
            _groupService = groupService;
            _mailService = mailService;
            _getUserByIdQuery = getUserByIdQuery;
            _randomSampleService = randomSampleService;
            _findOtherUsersQuery = findOtherUsersQuery;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Handle(SendObjectRequestedNotificationRequested e) {
            if (e.Status == ObjectRequestStatus.BlockedForForbiddenWords) {
                _orchardServices.Notifier.Add(NotifyType.Warning, T("Thank you for your request. We noticed some words that might be considered offensive. If our system flagged this incorrectly, we will send your request to the members of your group."));
                return;
            }

            var requestingUser = _getUserByIdQuery.GetResult(e.RequestingUserId);

            SendObjectRequestMail(requestingUser, e.Description, e.ExtraInfo, e.SourceId);
            _orchardServices.Notifier.Add(NotifyType.Success, T("Thank you for your request. We sent your request to the members of your group."));

            _mailService.SendAdminObjectRequestMail(requestingUser.GetUserDisplayName(), e.Description, e.ExtraInfo);
        }

        public void Handle(ObjectRequestUnblocked e) {
            if (e.WasPreviouslyBlockedByAdmin) {
                return;
            }

            var requestingUser = _getUserByIdQuery.GetResult(e.UserId);
            SendObjectRequestMail(requestingUser, e.Description, e.ExtraInfo, e.SourceId);
        }

        public void Handle(ObjectRequestBlocked e) {
            var requestingUser = _getUserByIdQuery.GetResult(e.UserId);
            _mailService.SendAdminObjectRequestBlockedMail(e.SourceId, requestingUser.GetUserDisplayName(), e.Description, e.ExtraInfo, e.ForbiddenWords);
        }

        public void Handle(ObjectRequestBlockedByAdmin e) {
            var requestingUser = _getUserByIdQuery.GetResult(e.UserId);
            _mailService.SendObjectRequestBlockedMail(requestingUser, e.SourceId, e.Description, e.Reason);
        }

        private void SendObjectRequestMail(IUser requestingUser, string description, string extraInfo, Guid sourceId) {
            var objectRequestMail = new ObjectRequestMail(
                Guid.NewGuid(),
                requestingUser.Id,
                description,
                extraInfo,
                sourceId);
            
            var requestingUserName = requestingUser.GetUserDisplayName();
            var groupName = _groupService.GetGroupForUser(requestingUser.Id).Name;
            var otherUsers = _findOtherUsersQuery.GetResults(requestingUser.Id, description).ToList();

            var recipients = _randomSampleService.GetRandomSample(otherUsers, 50).Where(x => x.As<UserDetailsPart>().ReceiveMails && x.As<GroupMembershipPart>().GroupMembershipStatus == GroupMembershipStatus.Approved);

            _mailService.SendObjectRequestMail(
                requestingUserName,
                groupName,
                sourceId,
                description,
                extraInfo,
                objectRequestMail,
                recipients.ToArray());

            _repository.Save(objectRequestMail, Guid.NewGuid().ToString());
        }
    }
}