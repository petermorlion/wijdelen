using System;
using System.Collections.Generic;
using Orchard;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.Entities;

namespace WijDelen.ObjectSharing.Infrastructure {
    public interface IMailService : IDependency {
        void SendObjectRequestMail(string requestingUserName, string groupName, Guid objectRequestId, string description, string extraInfo, ObjectRequestMail objectRequestMail, params IUser[] users);
        void SendChatMessageAddedMail(string culture, string fromUserName, string description, string toEmailAddress, Guid chatId, string message);
        void SendAdminObjectRequestBlockedMail(string requestingUserName, string description, string extraInfo, IList<string> forbiddenWords);
        void SendAmindObjectRequestMail(string requestingUserName, string description, string extraInfo);
    }
}