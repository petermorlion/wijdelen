using Orchard.ContentManagement;
using Orchard.Security;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public class GetUserByIdQuery : IGetUserByIdQuery {
        private readonly IContentManager _contentManager;

        public GetUserByIdQuery(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IUser GetResult(int userId) {
            return _contentManager.Get<IUser>(userId);
        }
    }
}