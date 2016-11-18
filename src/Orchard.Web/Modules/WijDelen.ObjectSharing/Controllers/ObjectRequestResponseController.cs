using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Themes;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    public class ObjectRequestResponseController : Controller {
        private readonly IRepository<ObjectRequestRecord> _repository;
        private readonly IFindSynonymsByExactMatchQuery _findSynonymsByExactMatchQuery;
        private readonly IOrchardServices _orchardServices;
        private readonly ICommandHandler<MarkSynonymAsOwned> _markSynonymAsOwnedCommandHandler;

        public ObjectRequestResponseController(
            IRepository<ObjectRequestRecord> repository,
            IOrchardServices orchardServices,
            IFindSynonymsByExactMatchQuery findSynonymsByExactMatchQuery,
            ICommandHandler<MarkSynonymAsOwned> markSynonymAsOwnedCommandHandler) {
            _repository = repository;
            _orchardServices = orchardServices;
            _findSynonymsByExactMatchQuery = findSynonymsByExactMatchQuery;
            _markSynonymAsOwnedCommandHandler = markSynonymAsOwnedCommandHandler;
        }

        public ActionResult NoFor(Guid id) {
            var record = _repository.Get(x => x.AggregateId == id);

            if (record == null) {
                return new HttpNotFoundResult();
            }

            return View();
        }

        public ActionResult YesFor(Guid id) {
            var record = _repository.Get(x => x.AggregateId == id);

            if (record == null) {
                return new HttpNotFoundResult();
            }

            var currentUser = _orchardServices.WorkContext.CurrentUser;
            var synonym = _findSynonymsByExactMatchQuery.GetResults(record.Description).FirstOrDefault();

            if (synonym != null) {
                var command = new MarkSynonymAsOwned(currentUser.Id, synonym.Id, synonym.As<TitlePart>().Title);
                _markSynonymAsOwnedCommandHandler.Handle(command);
            }

            return View();
        }

        public ActionResult NotNowFor(Guid id) {
            var record = _repository.Get(x => x.AggregateId == id);

            if (record == null) {
                return new HttpNotFoundResult();
            }

            return View();
        }
    }
}