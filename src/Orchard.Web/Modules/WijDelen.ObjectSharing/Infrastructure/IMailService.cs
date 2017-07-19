using System;
using System.Collections.Generic;
using Orchard;
using Orchard.Security;
using WijDelen.ObjectSharing.Domain.Entities;

namespace WijDelen.ObjectSharing.Infrastructure {
    public interface IMailService : IDependency {
        void SendObjectRequestMail(string requestingUserName, string groupName, Guid objectRequestId, string description, string extraInfo, ObjectRequestMail objectRequestMail, params IUser[] users);
        void SendChatMessageAddedMail(string culture, string fromUserName, string description, string toEmailAddress, Guid chatId, string message);
        void SendAdminObjectRequestBlockedMail(Guid objectRequestId, string requestingUserName, string description, string extraInfo, IList<string> forbiddenWords);
        void SendAdminObjectRequestMail(string requestingUserName, string description, string extraInfo);
        void SendObjectRequestBlockedMail(IUser requestingUser, Guid sourceId, string description, string reason);
    }
}