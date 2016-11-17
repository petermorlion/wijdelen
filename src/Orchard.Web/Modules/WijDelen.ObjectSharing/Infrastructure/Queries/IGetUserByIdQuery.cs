using Orchard;
using Orchard.Security;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public interface IGetUserByIdQuery : IDependency {
        IUser GetResult(int userId);
    }
}