using Orchard.ContentManagement.Records;

namespace WijDelen.UserImport.Models {
    public class OneTimeLoginPartRecord : ContentPartRecord {
        public virtual int UserId { get; set; }
        public virtual string Token { get; set; }
    }
}