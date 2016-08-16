using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Security;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Services {
    public class GroupService : IGroupService {
        private readonly IContentManager _contentManager;

        public GroupService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void AddUsersToGroup(string groupName, IEnumerable<IUser> users) {
            var group = _contentManager.New("Group");
            group.As<NamePart>().Name = groupName;
        }

        public string GetGroupName(IContent group) {
            return "TESTING";
        }

        public IEnumerable<GroupViewModel> GetGroups() {
            return new GroupViewModel[] {
                
            };
        }

        public void UpdateGroupMembershipForContentItem(ContentItem item, EditGroupMembershipViewModel model) {
            var groupMembershipPart = item.As<GroupMembershipPart>();
            groupMembershipPart.Group = _contentManager.Get(model.GroupId);
        }
    }
}