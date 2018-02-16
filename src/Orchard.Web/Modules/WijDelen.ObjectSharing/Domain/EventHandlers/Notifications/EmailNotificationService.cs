using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using IMailService = WijDelen.ObjectSharing.Infrastructure.IMailService;

namespace WijDelen.ObjectSharing.Domain.EventHandlers.Notifications {
    public class EmailNotificationService : IUserNotificationService {
        private readonly IMailService _mailService;
        private readonly IGetUserByIdQuery _getUserByIdQuery;
        private readonly IGroupService _groupService;

        public EmailNotificationService(IMailService mailService, IGetUserByIdQuery getUserByIdQuery, IGroupService groupService) {
            _mailService = mailService;
            _getUserByIdQuery = getUserByIdQuery;
            _groupService = groupService;
        }

        public void Handle(IEnumerable<IUser> users, ObjectRequested e) {
            SendObjectRequestedMails(users, e.UserId, e.SourceId, e.Description, e.ExtraInfo);
        }

        public void Handle(IEnumerable<IUser> users, ObjectRequestUnblocked e) {
            SendObjectRequestedMails(users, e.UserId, e.SourceId, e.Description, e.ExtraInfo);
        }

        private void SendObjectRequestedMails(IEnumerable<IUser> users, int requestingUserId, Guid objectRequestId, string description, string extraInfo) {
            var requestingUser = _getUserByIdQuery.GetResult(requestingUserId);
            var groupName = _groupService.GetGroupForUser(requestingUser.Id).Name;

            var subscribedUsers = users.Where(x => x.As<UserDetailsPart>().ReceiveMails).ToList();

            _mailService.SendObjectRequestMail(
                requestingUser.GetUserDisplayName(),
                groupName,
                objectRequestId,
                description,
                extraInfo,
                null,
                subscribedUsers.ToArray());
        }
    }
}