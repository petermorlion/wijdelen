using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Security;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public class FindUsersByIdsQuery : IFindUsersByIdsQuery {
        private readonly IContentManager _contentManager;

        public FindUsersByIdsQuery(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<IUser> GetResult(params int[] userIds) {
            return _contentManager.GetMany<IUser>(userIds, VersionOptions.Latest, QueryHints.Empty);
        }
    }
}