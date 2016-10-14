using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Infrastructure.Queries;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class SynonymGenerator : IEventHandler<ObjectRequested> {
        private readonly IContentManager _contentManager;
        private readonly IFindSynonymsByExactMatchQuery _findSynonymsByExactMatchQuery;

        public SynonymGenerator(IContentManager contentManager, IFindSynonymsByExactMatchQuery findSynonymsByExactMatchQuery) {
            _contentManager = contentManager;
            _findSynonymsByExactMatchQuery = findSynonymsByExactMatchQuery;
        }

        public void Handle(ObjectRequested e) {
            var existingSynonyms = _findSynonymsByExactMatchQuery.GetResults(e.Description);
            if (existingSynonyms.Any()) {
                return;
            }

            var synonym = _contentManager.New("Synonym");
            synonym.As<TitlePart>().Title = e.Description;
            _contentManager.Create(synonym);
            _contentManager.Publish(synonym);
        }
    }
}