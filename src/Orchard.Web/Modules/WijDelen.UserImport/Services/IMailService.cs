using System;
using System.Collections.Generic;
using Orchard;
using Orchard.Security;

namespace WijDelen.UserImport.Services
{
    public interface IMailService : IDependency {
        void SendUserVerificationMails(IEnumerable<IUser> users, Func<string, string> createUrl, string groupName, string groupLogoUrl);
    }
}