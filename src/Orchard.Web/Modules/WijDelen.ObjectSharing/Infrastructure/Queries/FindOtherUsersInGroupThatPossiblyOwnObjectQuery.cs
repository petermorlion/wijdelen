using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Security;
using Orchard.Users.Models;
using WijDelen.ObjectSharing.Models;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public class FindOtherUsersInGroupThatPossiblyOwnObjectQuery : IFindOtherUsersInGroupThatPossiblyOwnObjectQuery {
        private readonly IContentManager _contentManager;

        public FindOtherUsersInGroupThatPossiblyOwnObjectQuery(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<IUser> GetOtherUsersInGroup(int userId) {
            var user = _contentManager.Query().ForType("User").Where<UserPartRecord>(x => x.Id == userId).List().Single();
            var group = user.As<GroupMembershipPart>().Group;
            var result = _contentManager
                .Query<UserPart, UserPartRecord>()
                .Where<GroupMembershipPartRecord>(x => x.Group.Id == group.Id)
                .List()
                .Select(x => x.ContentItem.As<IUser>())
                .Where(x => x.Id != userId)
                .ToList();

            return result;
        }
    }
}