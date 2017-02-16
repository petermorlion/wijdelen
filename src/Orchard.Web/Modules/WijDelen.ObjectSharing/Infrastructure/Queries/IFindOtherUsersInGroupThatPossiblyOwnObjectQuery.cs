using System.Collections.Generic;
using Orchard;
using Orchard.Security;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    /// <summary>
    /// Finds all users in a group without the given user and without
    /// users that have indicated they don't own a certain object.
    /// </summary>
    public interface IFindOtherUsersInGroupThatPossiblyOwnObjectQuery : IDependency {
        IEnumerable<IUser> GetResults(int userId, string description);
    }
}