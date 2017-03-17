using Orchard.ContentManagement;
using Orchard.Security;

namespace WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes {
    public class FakeUser : IUser {
        public ContentItem ContentItem { get; set;  }
        public int Id { get; set;  }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}