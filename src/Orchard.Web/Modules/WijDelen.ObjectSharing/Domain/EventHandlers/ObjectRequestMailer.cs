using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.UserImport.Services;
using IMailService = WijDelen.ObjectSharing.Infrastructure.IMailService;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ObjectRequestMailer : IEventHandler<ObjectRequested>, IEventHandler<ObjectRequestMailCreated>
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

            _repository.Save(objectRequestMail, Guid.NewGuid().ToString());
        }

        public void Handle(ObjectRequestMailCreated objectRequestMailCreated) {

            var requestingUserName = _getUserByIdQuery.GetResult(objectRequestMailCreated.UserId).UserName;
            var groupName = _groupService.GetGroupNameForUser(objectRequestMailCreated.UserId);
            var otherUsers = _groupService.GetOtherUsersInGroup(objectRequestMailCreated.UserId);
            var emailAddresses = otherUsers.Select(x => x.Email).ToArray();

            _mailService.SendObjectRequestMail(
                requestingUserName,
                groupName,
                objectRequestMailCreated.Description, 
                objectRequestMailCreated.ExtraInfo,
                emailAddresses);

            var objectRequestMail = _repository.Find(objectRequestMailCreated.SourceId);
            objectRequestMail.MarkAsSent(emailAddresses, T("object-request-mail-html", requestingUserName, groupName, objectRequestMailCreated.Description, objectRequestMailCreated.ExtraInfo).ToString());
            _repository.Save(objectRequestMail, Guid.NewGuid().ToString());
        }
    }
}