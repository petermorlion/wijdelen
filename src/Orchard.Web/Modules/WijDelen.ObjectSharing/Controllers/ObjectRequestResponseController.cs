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
        private readonly IOrchardServices _orchardServices;
        private readonly ICommandHandler<ConfirmObjectRequest> _confirmObjectRequestCommandHandler;

        public ObjectRequestResponseController(
            IRepository<ObjectRequestRecord> repository,
            IOrchardServices orchardServices,
            ICommandHandler<ConfirmObjectRequest> confirmObjectRequestCommandHandler) {
            _repository = repository;
            _orchardServices = orchardServices;
            _confirmObjectRequestCommandHandler = confirmObjectRequestCommandHandler;
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

            var command = new ConfirmObjectRequest(currentUser.Id, id);
            _confirmObjectRequestCommandHandler.Handle(command);

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