using System.Collections.Generic;
using Orchard;
using Orchard.Security;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public interface IFindUsersByIdsQuery : IDependency {
        IEnumerable<IUser> GetResult(params int[] userIds);
    }
}