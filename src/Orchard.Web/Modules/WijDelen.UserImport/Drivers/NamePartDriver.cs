using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Drivers {
    public class NamePartDriver : ContentPartDriver<NamePart> {
        protected override DriverResult Display(NamePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_Name", () => shapeHelper.Parts_Name());
        }

        protected override DriverResult Editor(NamePart part, dynamic shapeHelper) {
            return ContentShape("Parts_Name_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Name",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(NamePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}