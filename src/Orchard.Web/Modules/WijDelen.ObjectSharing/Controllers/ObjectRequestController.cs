using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;
using WijDelen.Mobile;
using WijDelen.Mobile.Providers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    [HybridAuthorize]
    public class ObjectRequestController : Controller {
        public const int MaximumDescriptionLength = 50;
        public const int MaximumExtraInfoLength = 1000;
        private readonly IRepository<ChatRecord> _chatRepository;
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly ICommandHandler<RequestObject> _requestObjectCommandHandler;
        private readonly ICommandHandler<StopObjectRequest> _stopObjectRequestCommandHandler;

        public ObjectRequestController(
            ICommandHandler<RequestObject> requestObjectCommandHandler,
            ICommandHandler<StopObjectRequest> stopObjectRequestCommandHandler,
            IRepository<ObjectRequestRecord> objectRequestRepository,
            IRepository<ChatRecord> chatRepository,
            IOrchardServices orchardServices) {
            _requestObjectCommandHandler = requestObjectCommandHandler;
            _stopObjectRequestCommandHandler = stopObjectRequestCommandHandler;
            _objectRequestRepository = objectRequestRepository;
            _chatRepository = chatRepository;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult New() {
            return View(new NewObjectRequestViewModel());
        }

        [HttpPost]
        public ActionResult New(NewObjectRequestViewModel viewModel) {
            if (string.IsNullOrWhiteSpace(viewModel.Description)) ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.Description, T("Please provide a description of the item you need."));
            else if (viewModel.Description.Length > MaximumDescriptionLength) ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.Description, T("Please limit your description to {0} characters.", MaximumDescriptionLength));

            if (string.IsNullOrWhiteSpace(viewModel.ExtraInfo)) ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.ExtraInfo, T("Please provide some extra info."));
            else if (viewModel.Description.Length > MaximumDescriptionLength) ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.ExtraInfo, T("Please limit the extra info to {0} characters.", MaximumExtraInfoLength));

            if (!string.IsNullOrWhiteSpace(viewModel.ExtraInfo) && viewModel.ExtraInfo.Length < 30) ModelState.AddModelError<NewObjectRequestViewModel, string>(m => m.ExtraInfo, T("Please provide some more extra info (at least 30 characters)."));

            if (!ModelState.IsValid) return this.ViewOrJson(viewModel);

            var currentUser = _orchardServices.WorkContext.CurrentUser;

            var command = new RequestObject(viewModel.Description, viewModel.ExtraInfo, currentUser.Id);
            _requestObjectCommandHandler.Handle(command);

            return RedirectToAction("Item", new {id = command.ObjectRequestId});
        }

        public ActionResult Item(Guid id) {
            var record = _objectRequestRepository.Get(x => x.AggregateId == id);
            var chatRecords = _chatRepository.Fetch(x => x.ObjectRequestId == id).ToList();

            if (record == null) return new HttpNotFoundResult();

            if (record.UserId != _orchardServices.WorkContext.CurrentUser.Id) return new HttpUnauthorizedResult();

            if (record.Status == "BlockedForForbiddenWords") _orchardServices.Notifier.Add(NotifyType.Warning, T("This request was blocked because it contains words that may be considered offensive or inappropriate."));

            if (record.Status == "BlockedByAdmin") _orchardServices.Notifier.Add(NotifyType.Warning, T("This request was blocked by the administrator:\n\n{0}", record.BlockReason));

            if (record.Status == "Stopped") _orchardServices.Notifier.Add(NotifyType.Warning, T("This request has been stopped. You can not add any more messages and other users can no longer respond to this request."));

            var viewModel = new ObjectRequestViewModel {
                ObjectRequestRecord = record,
                ChatRecords = chatRecords
            };

            return this.ViewOrJson(viewModel);
        }

        public ActionResult Index() {
            var records = _objectRequestRepository
                .Fetch(x => x.UserId == _orchardServices.WorkContext.CurrentUser.Id)
                .OrderByDescending(x => x.CreatedDateTime).ToList();

            var viewModels = records.Select(x => new IndexObjectRequestViewModel {
                AggregateId = x.AggregateId,
                Status = x.Status,
                Description = x.Description,
                BlockReason = x.BlockReason
            });

            var viewModel = new ObjectRequestsIndexViewModel {Data = viewModels};

            return this.ViewOrJson(viewModel);
        }

        public ActionResult Stop(Guid id) {
            var record = _objectRequestRepository.Fetch(x => x.AggregateId == id).SingleOrDefault();

            if (record == null) {
                return new HttpNotFoundResult();
            }

            if (record.UserId != _orchardServices.WorkContext.CurrentUser.Id) {
                return new HttpUnauthorizedResult();
            }

            return this.ViewOrJson(new ConfirmStopObjectRequestViewModel {Id = id, Description = record.Description});
        }

        [HttpPost]
        public ActionResult Stop(ConfirmStopObjectRequestViewModel confirmStopObjectRequestViewModel) {
            var command = new StopObjectRequest(confirmStopObjectRequestViewModel.Id);
            _stopObjectRequestCommandHandler.Handle(command);

            return RedirectToAction("Index");
        }
    }
}