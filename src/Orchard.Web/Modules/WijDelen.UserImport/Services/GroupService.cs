using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Security;
using WijDelen.UserImport.Models;

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
    }
}