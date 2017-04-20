using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    [Authorize]
    public class ObjectRequestResponseController : Controller {
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly IRepository<ChatRecord> _chatRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly ICommandHandler<ConfirmObjectRequest> _confirmObjectRequestCommandHandler;
        private readonly ICommandHandler<DenyObjectRequest> _denyObjectRequestCommandHandler;
        private readonly ICommandHandler<DenyObjectRequestForNow> _denyObjectRequestForNowCommandHandler;

        public ObjectRequestResponseController(
            IRepository<ObjectRequestRecord> objectRequestRepository,
            IRepository<ChatRecord> chatRepository,
            IOrchardServices orchardServices,
            ICommandHandler<ConfirmObjectRequest> confirmObjectRequestCommandHandler,
            ICommandHandler<DenyObjectRequest> denyObjectRequestCommandHandler,
            ICommandHandler<DenyObjectRequestForNow> denyObjectRequestForNowCommandHandler) {
            _objectRequestRepository = objectRequestRepository;
            _chatRepository = chatRepository;
            _orchardServices = orchardServices;
            _confirmObjectRequestCommandHandler = confirmObjectRequestCommandHandler;
            _denyObjectRequestCommandHandler = denyObjectRequestCommandHandler;
            _denyObjectRequestForNowCommandHandler = denyObjectRequestForNowCommandHandler;

            T = NullLocalizer.Instance;
        }

        public ActionResult Deny(Guid id) {
            var record = _objectRequestRepository.Get(x => x.AggregateId == id);

            if (record == null) {
                return new HttpNotFoundResult();
            }

            var currentUser = _orchardServices.WorkContext.CurrentUser;

            var command = new DenyObjectRequest(currentUser.Id, id);
            _denyObjectRequestCommandHandler.Handle(command);

            return View();
        }

        public ActionResult Confirm(Guid id) {
            var record = _objectRequestRepository.Get(x => x.AggregateId == id);

            if (record == null) {
                return new HttpNotFoundResult();
            }

            var currentUser = _orchardServices.WorkContext.CurrentUser;

            var command = new ConfirmObjectRequest(currentUser.Id, id);
            _confirmObjectRequestCommandHandler.Handle(command);

            var chatRecord = _chatRepository.Fetch(x => x.ObjectRequestId == id && x.ConfirmingUserId == currentUser.Id).Single();

            _orchardServices.Notifier.Add(NotifyType.Success, T("Thank you for your response. You can now chat with {0}.", chatRecord.RequestingUserName));
            
            return RedirectToAction("Index", "Chat", new {id = chatRecord.ChatId});
        }

        public ActionResult DenyForNow(Guid id) {
            var record = _objectRequestRepository.Get(x => x.AggregateId == id);

            if (record == null) {
                return new HttpNotFoundResult();
            }

            var currentUser = _orchardServices.WorkContext.CurrentUser;

            var command = new DenyObjectRequestForNow(currentUser.Id, id);
            _denyObjectRequestForNowCommandHandler.Handle(command);

            return View();
        }

        public Localizer T { get; set; }
    }
}