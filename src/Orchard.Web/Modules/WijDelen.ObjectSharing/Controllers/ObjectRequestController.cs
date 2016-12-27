using System;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    public class ObjectRequestController : Controller {
        private readonly ICommandHandler<RequestObject> _requestObjectCommandHandler;
        private readonly IRepository<ObjectRequestRecord> _repository;
        private readonly IOrchardServices _orchardServices;

        public ObjectRequestController(
            ICommandHandler<RequestObject> requestObjectCommandHandler,
            IRepository<ObjectRequestRecord> repository,
            IOrchardServices orchardServices) {
            _requestObjectCommandHandler = requestObjectCommandHandler;
            _repository = repository;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        [Authorize]
        public ActionResult New() {
            return View(new NewObjectRequestViewModel());
        }

        [Authorize]
        [HttpPost]
        public ActionResult New(NewObjectRequestViewModel viewModel) {
            if (string.IsNullOrWhiteSpace(viewModel.Description)) {
                ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.Description, T("Please provide a description of the item you need."));
            }

            if (string.IsNullOrWhiteSpace(viewModel.ExtraInfo)) {
                ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.ExtraInfo, T("Please provide some extra info."));
            }

            if (!ModelState.IsValid) {
                return View(viewModel);
            }

            var currentUser = _orchardServices.WorkContext.CurrentUser;

            var command = new RequestObject(viewModel.Description, viewModel.ExtraInfo, currentUser.Id);
            _requestObjectCommandHandler.Handle(command);

            return RedirectToAction("Item", new {id = command.ObjectRequestId});
        }

        [Authorize]
        public ActionResult Item(Guid id) {
            var record = _repository.Get(x => x.AggregateId == id);

            if (record == null) {
                return new HttpNotFoundResult();
            }

            if (record.UserId != _orchardServices.WorkContext.CurrentUser.Id) {
                return new HttpUnauthorizedResult();
            }

            return View(record);
        }

        public Localizer T { get; set; }
    }
}