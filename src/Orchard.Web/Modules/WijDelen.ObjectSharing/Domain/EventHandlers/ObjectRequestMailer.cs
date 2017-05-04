using System;
using System.Linq;
using Orchard;
using Orchard.Localization;
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
    public class ObjectRequestMailer : IEventHandler<ObjectRequested>, IEventHandler<ObjectRequestUnblocked>
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

        public void Handle(ObjectRequested objectRequested) {
            if (objectRequested.Status == ObjectRequestStatus.BlockedForForbiddenWords)
            {
                _orchardServices.Notifier.Add(NotifyType.Warning, T("Thank you for your request. We noticed some words that might be considered offensive. If our system flagged this incorrectly, we will send your request to the members of your group."));
                return;
            }

            SendObjectRequestMail(objectRequested.UserId, objectRequested.Description, objectRequested.ExtraInfo, objectRequested.SourceId, objectRequested.Status);
            _orchardServices.Notifier.Add(NotifyType.Success, T("Thank you for your request. We sent your request to the members of your group."));
        }

        public void Handle(ObjectRequestUnblocked e) {
            SendObjectRequestMail(e.UserId, e.Description, e.ExtraInfo, e.SourceId, e.Status);
        }

        private void SendObjectRequestMail(int userId, string description, string extraInfo, Guid sourceId, ObjectRequestStatus objectRequestStatus) {
            var objectRequestMail = new ObjectRequestMail(
                Guid.NewGuid(),
                userId,
                description,
                extraInfo,
                sourceId);

            var requestingUser = _getUserByIdQuery.GetResult(userId);
            var requestingUserName = requestingUser.GetUserDisplayName();
            var groupName = _groupService.GetGroupForUser(userId).Name;
            var otherUsers = _findOtherUsersQuery.GetResults(userId, description).ToList();

            var recipients = _randomSampleService.GetRandomSample(otherUsers, 250);

            var userEmails = recipients.Select(x => new UserEmail {UserId = x.Id, Email = x.Email}).ToArray();

            _mailService.SendObjectRequestMail(
                requestingUserName,
                groupName,
                sourceId,
                description,
                extraInfo,
                objectRequestMail,
                userEmails);

            _repository.Save(objectRequestMail, Guid.NewGuid().ToString());
        }
    }
}