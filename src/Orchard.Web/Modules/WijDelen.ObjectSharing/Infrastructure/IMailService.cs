using System;
using Orchard;
using WijDelen.ObjectSharing.Domain.Entities;

namespace WijDelen.ObjectSharing.Infrastructure {
    public interface IMailService : IDependency {
        void SendObjectRequestMail(string requestingUserName, string groupName, Guid objectRequestId, string description, string extraInfo, ObjectRequestMail objectRequestMail, params string[] emailAddresses);
        void SendChatMessageAddedMail(string fromUserName, string toUserName, string description, string toEmailAddress, Guid chatId, string message);
    }
}