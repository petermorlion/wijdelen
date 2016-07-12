using System;
using System.Collections.Generic;
using Orchard;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services
{
    public interface IMailService : IDependency {
        void SendUserVerificationMails(IEnumerable<UserImportResult> userImportResults, Func<string, string> createUrl);
    }
}