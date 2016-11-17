using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Models;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Services {
    public class GroupService : IGroupService {
        private readonly IContentManager _contentManager;

        public GroupService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void AddUsersToGroup(string groupName, IEnumerable<IUser> users) {
            var group = _contentManager.Query().ForType("Group").Where<NamePartRecord>(x => x.Name == groupName).List().FirstOrDefault();
            if (group == null) {
                group = _contentManager.New("Group");
                group.As<NamePart>().Name = groupName;
                _contentManager.Create(group);
                _contentManager.Publish(group);
            }

            foreach (var user in users) {
                user.As<GroupMembershipPart>().Group = group;
            }
        }

        public string GetGroupName(IContent group) {
            return group.As<NamePart>().Name;
        }

        public IEnumerable<GroupViewModel> GetGroups() {
            return _contentManager.Query().ForType("Group").List().Select(x => new GroupViewModel {
                Id = x.Id,
                Name = x.As<NamePart>().Name
            });
        }

        public void UpdateGroupMembershipForContentItem(ContentItem item, EditGroupMembershipViewModel model) {
            var groupMembershipPart = item.As<GroupMembershipPart>();
            groupMembershipPart.Group = _contentManager.Get(model.GroupId);
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