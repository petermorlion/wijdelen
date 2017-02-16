using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Security;
using Orchard.Users.Models;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.ObjectSharing.Models;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Infrastructure.Queries {
    public class FindOtherUsersInGroupThatPossiblyOwnObjectQuery : IFindOtherUsersInGroupThatPossiblyOwnObjectQuery {
        private readonly IContentManager _contentManager;
        private readonly IRepository<UserInventoryRecord> _userInventoryRepository;

        public FindOtherUsersInGroupThatPossiblyOwnObjectQuery(
            IContentManager contentManager, 
            IRepository<UserInventoryRecord> userInventoryRepository) {
            _contentManager = contentManager;
            _userInventoryRepository = userInventoryRepository;
        }

        public IEnumerable<IUser> GetResults(int userId, string description) {
            var user = _contentManager.Query().ForType("User").Where<UserPartRecord>(x => x.Id == userId).List().Single();
            var group = user.As<GroupMembershipPart>().Group;
            var otherUsersInGroup = _contentManager
                .Query<UserPart, UserPartRecord>()
                .Where<GroupMembershipPartRecord>(x => x.Group.Id == group.Id)
                .List()
                .Select(x => x.ContentItem.As<IUser>())
                .Where(x => x.Id != userId)
                .ToList();

            var archetypes = _contentManager
                .Query("Archetype")
                .List()
                .Where(x => x.As<TitlePart>().Title == description)
                .ToList();

            if (!archetypes.Any()) {
                return otherUsersInGroup;
            }

            var synonyms = _contentManager
                .Query("Synonym")
                .List()
                .ToList();

            var userIds = otherUsersInGroup.Select(x => x.Id).ToList();
            var usersThatSaidNo = _userInventoryRepository
                .Fetch(x => userIds.Contains(x.UserId) && x.Answer == ObjectRequestAnswer.No)
                .GroupBy(x => x.SynonymId, x => x.UserId);

            foreach (var grouping in usersThatSaidNo) {
                var synonym = synonyms.SingleOrDefault(x => x.Id == grouping.Key);
                if (synonym == null) {
                    return otherUsersInGroup;
                }

                var archetypeId = ((ContentPickerField) ((ContentPart) synonym.Content.Synonym).Get(typeof(ContentPickerField), "Archetype")).Ids.FirstOrDefault();
                if (archetypes.Any(x => x.Id == archetypeId)) {
                    otherUsersInGroup.RemoveAll(x => grouping.Contains(x.Id));
                }
            }

            return otherUsersInGroup;
        }
    }
}