using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class SynonymGenerator : IEventHandler<ObjectRequested> {
        private readonly IContentManager _contentManager;

        public SynonymGenerator(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void Handle(ObjectRequested e) {
            var synonym = _contentManager.New("Synonym");
            synonym.As<TitlePart>().Title = e.Description;
            _contentManager.Create(synonym);
            _contentManager.Publish(synonym);
        }
    }
}