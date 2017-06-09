using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Fields;
using Orchard.Security;
using Orchard.Users.Models;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Services {
    public class GroupService : IGroupService {
        private readonly IOrchardServices _orchardServices;

        public GroupService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public void AddUsersToGroup(string groupName, IEnumerable<IUser> users) {
            var group = _orchardServices.ContentManager.Query().ForType("Group").Where<NamePartRecord>(x => x.Name == groupName).List().FirstOrDefault();
            if (group == null) {
                group = _orchardServices.ContentManager.New("Group");
                group.As<NamePart>().Name = groupName;
                _orchardServices.ContentManager.Create(group);
                _orchardServices.ContentManager.Publish(group);
            }

            foreach (var user in users) {
                user.As<GroupMembershipPart>().Group = group;
                user.As<GroupMembershipPart>().GroupMembershipStatus = GroupMembershipStatus.Pending;
            }
        }

        public string GetGroupName(IContent group) {
            return group.As<NamePart>().Name;
        }

        public IEnumerable<GroupViewModel> GetGroups() {
            var result = new List<GroupViewModel>();
            var groups = _orchardServices.ContentManager.Query().ForType("Group").List();
            foreach (var x in groups) {
                var groupViewModel = new GroupViewModel {
                    Id = x.Id,
                    Name = x.As<NamePart>().Name
                };

                var groupLogoField = x.Parts
                    .SingleOrDefault(p => p.PartDefinition.Name == "GroupLogoPart")
                    ?.Fields.SingleOrDefault(f => f.FieldDefinition.Name == "MediaLibraryPickerField") as MediaLibraryPickerField;

                if (groupLogoField != null) {
                    groupViewModel.LogoUrl = groupLogoField.FirstMediaUrl;
                }

                result.Add(groupViewModel);
            }

            return result;
        }

        public void UpdateGroupMembershipForContentItem(ContentItem item, EditGroupMembershipViewModel model) {
            var groupMembershipPart = item.As<GroupMembershipPart>();
            groupMembershipPart.Group = _orchardServices.ContentManager.Get(model.GroupId);
        }

        public GroupViewModel GetGroupForUser(int userId) {
            var user = _orchardServices.ContentManager.Query().ForType("User").Where<UserPartRecord>(x => x.Id == userId).List().Single();
            var group = user.As<GroupMembershipPart>().Group;
            var groupViewModel = group != null ? new GroupViewModel
            {
                Id = group.Id,
                Name = group.As<NamePart>().Name
            } : null;

            if (groupViewModel == null) {
                return null;
            }

            var groupLogoField = group.ContentItem.Parts
                    .SingleOrDefault(p => p.PartDefinition.Name == "GroupLogoPart")
                    ?.Fields.SingleOrDefault(f => f.FieldDefinition.Name == "MediaLibraryPickerField") as MediaLibraryPickerField;

            if (groupLogoField != null) {
                var url = _orchardServices.WorkContext.HttpContext.Request.Url;
                groupViewModel.LogoUrl = url.Scheme + "://" + url.Authority + groupLogoField.FirstMediaUrl;
            }

            return groupViewModel;
        }

        public IList<IUser> GetUsersInGroup(int groupId) {
            var result = new List<IUser>();
            foreach (var userPart in _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().List().ToList()) {
                result.Add(userPart);
            }

            return result;
        }
    }
}