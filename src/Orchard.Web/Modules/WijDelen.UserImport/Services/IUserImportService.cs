using System.Collections.Generic;
using Orchard;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services {
    public interface IUserImportService : IDependency {
        IList<UserImportResult> ImportUsers(string culture, IList<string> emails);
    }
}