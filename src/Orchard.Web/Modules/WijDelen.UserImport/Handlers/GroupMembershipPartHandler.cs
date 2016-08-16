using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Handlers {
    public class GroupMembershipPartHandler : ContentHandler
    {
        private readonly IContentManager _contentManager;

        public GroupMembershipPartHandler(IRepository<GroupMembershipPartRecord> repository, IContentManager contentManager)
        {
            Filters.Add(StorageFilter.For(repository));
            _contentManager = contentManager;

            OnInitializing<GroupMembershipPart>(PropertySetHandlers);
            OnLoaded<GroupMembershipPart>(LazyLoadHandlers);
        }

        void LazyLoadHandlers(LoadContentContext context, GroupMembershipPart part)
        {
            // add handlers that will load content just-in-time
            part.GroupField.Loader(() =>
                part.Record.Group == null ? null : _contentManager.Get(part.Record.Group.Id));
        }

        static void PropertySetHandlers(InitializingContentContext context, GroupMembershipPart part)
        {
            // add handlers that will update records when part properties are set
            part.GroupField.Setter(group => {
                part.Record.Group = group?.ContentItem.Record;
                return group;
            });

            // Force call to setter if we had already set a value
            if (part.GroupField.Value != null)
                part.GroupField.Value = part.GroupField.Value;
        }
    }
}