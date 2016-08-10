using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Handlers {
    public class NamePartHandler : ContentHandler {
        public NamePartHandler(IRepository<NamePartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }   
    }
}