using System.ComponentModel;
using Orchard.ContentManagement;

namespace WijDelen.UserImport.Models {
    public class OneTimeLoginPart : ContentPart<OneTimeLoginPartRecord> {
        [DisplayName("The user id of this one-time login")]
        public int UserId {
            get { return Retrieve(r => r.UserId); }
            set { Store(r => r.UserId, value); }
        }

        [DisplayName("The token of this one-time login")]
        public string Token
        {
            get { return Retrieve(r => r.Token); }
            set { Store(r => r.Token, value); }
        }
    }
}