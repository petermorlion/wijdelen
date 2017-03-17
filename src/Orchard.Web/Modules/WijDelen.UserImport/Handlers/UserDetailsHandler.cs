using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Handlers {
    public class UserDetailsHandler : ContentHandler {
        public UserDetailsHandler(IRepository<UserDetailsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}