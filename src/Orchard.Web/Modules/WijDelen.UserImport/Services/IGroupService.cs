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

        /// <summary>
        /// Gets all users in the group of the given user, excluding the given user.
        /// </summary>
        IEnumerable<IUser> GetOtherUsersInGroup(int userId);

        GroupViewModel GetGroupForUser(int userId);

        bool IsMemberOfGroup(int userId);
    }
}