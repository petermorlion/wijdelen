using Orchard.ContentManagement.Records;

namespace WijDelen.UserImport.Models {
    public class NamePartRecord : ContentPartRecord {
        public virtual string Name { get; set; }
    }
}