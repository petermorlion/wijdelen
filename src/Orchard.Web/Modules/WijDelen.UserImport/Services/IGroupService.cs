using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Services {
    public interface IGroupService : IDependency {
        void AddUsersToGroup(string groupName, IEnumerable<IUser> users);
        string GetGroupName(IContent group);
        IEnumerable<GroupViewModel> GetGroups();
        void UpdateGroupMembershipForContentItem(ContentItem contentItem, EditGroupMembershipViewModel model);

        GroupViewModel GetGroupForUser(int userId);

        bool IsMemberOfGroup(int userId);
    }
}