using System;
using System.Linq;
using Orchard.Localization;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.UserImport.Services;
using IMailService = WijDelen.ObjectSharing.Infrastructure.IMailService;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ObjectRequestMailer : IEventHandler<ObjectRequested>
    {
        private readonly IEventSourcedRepository<ObjectRequestMail> _repository;
        private readonly IGroupService _groupService;
        private readonly IMailService _mailService;
        private readonly IGetUserByIdQuery _getUserByIdQuery;

        public ObjectRequestMailer(
            IEventSourcedRepository<ObjectRequestMail> repository, 
            IGroupService groupService,
            IMailService mailService,
            IGetUserByIdQuery getUserByIdQuery) {
            _repository = repository;
            _groupService = groupService;
            _mailService = mailService;
            _getUserByIdQuery = getUserByIdQuery;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Handle(ObjectRequested objectRequested) {
            var objectRequestMail = new ObjectRequestMail(
                Guid.NewGuid(),
                objectRequested.UserId,
                objectRequested.Description,
                objectRequested.ExtraInfo);

            var requestingUserName = _getUserByIdQuery.GetResult(objectRequested.UserId).UserName;
            var groupName = _groupService.GetGroupNameForUser(objectRequested.UserId);
            var otherUsers = _groupService.GetOtherUsersInGroup(objectRequested.UserId);
            var emailAddresses = otherUsers.Select(x => x.Email).ToArray();

            _mailService.SendObjectRequestMail(
                requestingUserName,
                groupName,
                objectRequested.SourceId,
                objectRequested.Description,
                objectRequested.ExtraInfo,
                objectRequestMail,
                emailAddresses);

            _repository.Save(objectRequestMail, Guid.NewGuid().ToString());
        }
    }
}