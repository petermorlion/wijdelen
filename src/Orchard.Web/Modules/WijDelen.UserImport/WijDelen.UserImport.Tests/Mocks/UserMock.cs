using Orchard.ContentManagement;
using Orchard.Security;

namespace WijDelen.UserImport.Tests.Mocks {
    public class UserMock : IUser {
        public ContentItem ContentItem { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}