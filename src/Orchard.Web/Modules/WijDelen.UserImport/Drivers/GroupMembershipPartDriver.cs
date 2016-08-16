using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Drivers {
    public class GroupMembershipPartDriver : ContentPartDriver<GroupMembershipPart> {
        private readonly IGroupService _groupService;

        private const string TemplateName = "Parts/GroupMembership";

        public GroupMembershipPartDriver(IGroupService groupService)
        {
            _groupService = groupService;
        }

        protected override string Prefix => "Group";

        protected override DriverResult Display(GroupMembershipPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_GroupMembership",
                () => shapeHelper.Parts_GroupMembership(
                    ContentPart: part,
                    Group: part.Group,
                    GroupName: _groupService.GetGroupName(part.Group)
                    ));
        }

        protected override DriverResult Editor(GroupMembershipPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_GroupMembership_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: TemplateName,
                        Model: BuildEditorViewModel(part),
                        Prefix: Prefix));
        }

        protected override DriverResult Editor(GroupMembershipPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var model = new EditGroupMembershipViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            if (part.ContentItem.Id != 0)
            {
                _groupService.UpdateGroupMembershipForContentItem(
                    part.ContentItem, model);
            }

            return Editor(part, shapeHelper);
        }

        private EditGroupMembershipViewModel BuildEditorViewModel(GroupMembershipPart part)
        {
            var viewModel = new EditGroupMembershipViewModel
            {
                UserId = part.ContentItem.Id,
                Groups = _groupService.GetGroups()
            };

            if (part.Group != null)
            {
                viewModel.GroupId = part.Group.Id;
            }

            return viewModel;
        }
    }
}