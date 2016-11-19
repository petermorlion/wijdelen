using System;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Themes;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    public class ObjectRequestResponseController : Controller {
        private readonly IRepository<ObjectRequestRecord> _repository;
        private readonly IOrchardServices _orchardServices;
        private readonly ICommandHandler<ConfirmObjectRequest> _confirmObjectRequestCommandHandler;
        private readonly ICommandHandler<DenyObjectRequest> _denyObjectRequestCommandHandler;
        private readonly ICommandHandler<DenyObjectRequestForNow> _denyObjectRequestForNowCommandHandler;

        public ObjectRequestResponseController(
            IRepository<ObjectRequestRecord> repository,
            IOrchardServices orchardServices,
            ICommandHandler<ConfirmObjectRequest> confirmObjectRequestCommandHandler,
            ICommandHandler<DenyObjectRequest> denyObjectRequestCommandHandler,
            ICommandHandler<DenyObjectRequestForNow> denyObjectRequestForNowCommandHandler) {
            _repository = repository;
            _orchardServices = orchardServices;
            _confirmObjectRequestCommandHandler = confirmObjectRequestCommandHandler;
            _denyObjectRequestCommandHandler = denyObjectRequestCommandHandler;
            _denyObjectRequestForNowCommandHandler = denyObjectRequestForNowCommandHandler;
        }

        public ActionResult Deny(Guid id) {
            var record = _repository.Get(x => x.AggregateId == id);

            if (record == null) {
                return new HttpNotFoundResult();
            }

            var currentUser = _orchardServices.WorkContext.CurrentUser;

            var command = new DenyObjectRequest(currentUser.Id, id);
            _denyObjectRequestCommandHandler.Handle(command);

            return View();
        }

        public ActionResult Confirm(Guid id) {
            var record = _repository.Get(x => x.AggregateId == id);

            if (record == null) {
                return new HttpNotFoundResult();
            }

            var currentUser = _orchardServices.WorkContext.CurrentUser;

            var command = new ConfirmObjectRequest(currentUser.Id, id);
            _confirmObjectRequestCommandHandler.Handle(command);

            return View();
        }

        public ActionResult DenyForNow(Guid id) {
            var record = _repository.Get(x => x.AggregateId == id);

            if (record == null) {
                return new HttpNotFoundResult();
            }

            var currentUser = _orchardServices.WorkContext.CurrentUser;

            var command = new DenyObjectRequestForNow(currentUser.Id, id);
            _denyObjectRequestForNowCommandHandler.Handle(command);

            return View();
        }
    }
}