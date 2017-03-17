using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Drivers {
    public class UserDetailsPartDriver : ContentPartDriver<UserDetailsPart> {
        protected override DriverResult Display(UserDetailsPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_UserDetails", () => shapeHelper.Parts_UserDetails());
        }

        protected override DriverResult Editor(UserDetailsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_UserDetails_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/UserDetails",
                    Models: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(UserDetailsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}