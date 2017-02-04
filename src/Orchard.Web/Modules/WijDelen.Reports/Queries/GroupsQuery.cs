using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using WijDelen.Reports.ViewModels;
using WijDelen.UserImport.Models;

namespace WijDelen.Reports.Queries {
    public class GroupsQuery : IGroupsQuery {
        private readonly IContentManager _contentManager;

        public GroupsQuery(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<GroupViewModel> GetResults() {
            return _contentManager.Query().ForType("Group").List().Select(x => new GroupViewModel {
                Id = x.Id,
                Name = ContentExtensions.As<NamePart>(x).Name
            });
        }
    }
}