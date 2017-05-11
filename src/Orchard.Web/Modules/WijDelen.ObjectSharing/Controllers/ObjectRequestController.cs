using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using WijDelen.ObjectSharing.Data;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    [Authorize]
    public class ObjectRequestController : Controller {
        private readonly ICommandHandler<RequestObject> _requestObjectCommandHandler;
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly IRepository<ChatRecord> _chatRepository;
        private readonly IOrchardServices _orchardServices;

        private const int MaximumDescriptionLength = 50;
        private const int MaximumExtraInfoLength = 1000;

        public ObjectRequestController(
            ICommandHandler<RequestObject> requestObjectCommandHandler,
            IRepository<ObjectRequestRecord> objectRequestRepository,
            IRepository<ChatRecord> chatRepository,
            IOrchardServices orchardServices) {
            _requestObjectCommandHandler = requestObjectCommandHandler;
            _objectRequestRepository = objectRequestRepository;
            _chatRepository = chatRepository;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public ActionResult New() {
            return View(new NewObjectRequestViewModel());
        }

        [HttpPost]
        public ActionResult New(NewObjectRequestViewModel viewModel) {
            if (string.IsNullOrWhiteSpace(viewModel.Description)) {
                ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.Description, T("Please provide a description of the item you need."));
            } else if (viewModel.Description.Length > MaximumDescriptionLength) {
                ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.Description, T("Please limit your description to {0} characters.", MaximumDescriptionLength));
            }

            if (string.IsNullOrWhiteSpace(viewModel.ExtraInfo)) {
                ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.ExtraInfo, T("Please provide some extra info."));
            } else if (viewModel.Description.Length > MaximumDescriptionLength) {
                ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.ExtraInfo, T("Please limit the extra info to {0} characters.", MaximumExtraInfoLength));
            }

            if (!string.IsNullOrWhiteSpace(viewModel.ExtraInfo) && viewModel.ExtraInfo.Length < 30) {
                ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.ExtraInfo, T("Please provide some more extra info (at least 30 characters)."));
            }

            if (!ModelState.IsValid) {
                return View(viewModel);
            }

            var currentUser = _orchardServices.WorkContext.CurrentUser;

            var command = new RequestObject(viewModel.Description, viewModel.ExtraInfo, currentUser.Id);
            _requestObjectCommandHandler.Handle(command);

            return RedirectToAction("Item", new {id = command.ObjectRequestId});
        }

        public ActionResult Item(Guid id) {
            var record = _objectRequestRepository.Get(x => x.AggregateId == id);
            var chatRecords = _chatRepository.Fetch(x => x.ObjectRequestId == id).ToList();

            if (record == null) {
                return new HttpNotFoundResult();
            }

            if (record.UserId != _orchardServices.WorkContext.CurrentUser.Id) {
                return new HttpUnauthorizedResult();
            }

            if (record.Status == "BlockedForForbiddenWords") {
                return RedirectToAction("New");
            }

            var viewModel = new ObjectRequestViewModel {
                ObjectRequestRecord = record,
                ChatRecords = chatRecords
            };

            return View(viewModel);
        }

        public ActionResult Index() {
            var records = _objectRequestRepository
                .Fetch(x => x.UserId == _orchardServices.WorkContext.CurrentUser.Id
                    && x.Status != "BlockedForForbiddenWords")
                .OrderByDescending(x => x.CreatedDateTime).ToList();
            return View(records);
        }

        public Localizer T { get; set; }
    }
}