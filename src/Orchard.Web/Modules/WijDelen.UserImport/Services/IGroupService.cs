using System.Collections.Generic;
using Orchard;
using Orchard.Security;

namespace WijDelen.UserImport.Services {
    public interface IGroupService : IDependency {
        void AddUsersToGroup(string groupName, IEnumerable<IUser> users);
    }
}