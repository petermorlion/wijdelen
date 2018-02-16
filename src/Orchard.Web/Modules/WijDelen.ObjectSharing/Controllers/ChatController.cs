using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;
using WijDelen.Mobile;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    [Authorize]
    public class ChatController : Controller {
        private readonly ICommandHandler<AddChatMessage> _addChatMessageCommandHandler;
        private readonly IRepository<ChatMessageRecord> _chatMessageRepository;
        private readonly IRepository<ChatRecord> _chatRepository;
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly IOrchardServices _orchardServices;

        public ChatController(
            IRepository<ChatMessageRecord> chatMessageRepository,
            IRepository<ObjectRequestRecord> objectRequestRepository,
            ICommandHandler<AddChatMessage> addChatMessageCommandHandler,
            IRepository<ChatRecord> chatRepository,
            IOrchardServices orchardServices) {
            _chatMessageRepository = chatMessageRepository;
            _objectRequestRepository = objectRequestRepository;
            _addChatMessageCommandHandler = addChatMessageCommandHandler;
            _chatRepository = chatRepository;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /// <remarks>
        ///     Warning: also referenced hard-coded in WijDelen.ObjectSharing.MailgunService
        /// </remarks>
        public ActionResult Index(Guid id) {
            var chat = _chatRepository.Fetch(x => x.ChatId == id).SingleOrDefault();
            if (chat == null) return new HttpNotFoundResult();

            var chatViewModel = GetChatViewModel(id, chat);

            return this.ViewOrJson(chatViewModel);
        }

        /// <summary>
        /// Adds a message to the chat.
        /// </summary>
        /// <param name="postedViewModel">The viewmodel as posted by the browser. This will be an incomplete viewmodel, as not all properties
        /// of the viewmodel are present in the form.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(ChatViewModel postedViewModel) {
            var chat = _chatRepository.Fetch(x => x.ChatId == postedViewModel.ChatId).SingleOrDefault();
            if (chat == null) return new HttpNotFoundResult();

            var userId = _orchardServices.WorkContext.CurrentUser.Id;
            if (chat.ConfirmingUserId != userId && chat.RequestingUserId != userId) return new HttpUnauthorizedResult();

            var viewModel = GetChatViewModel(postedViewModel.ChatId, chat);

            if (string.IsNullOrWhiteSpace(postedViewModel.NewMessage))
                ModelState.AddModelError<ChatViewModel, string>(m => m.NewMessage, T("Please provide a message."));

            if (!ModelState.IsValid)
                return View(viewModel);

            var addChatMessage = new AddChatMessage(viewModel.ChatId, userId, postedViewModel.NewMessage, DateTime.UtcNow);
            _addChatMessageCommandHandler.Handle(addChatMessage);
            return RedirectToAction("Index", new {id = viewModel.ChatId});
        }

        private ChatViewModel GetChatViewModel(Guid id, ChatRecord chat) {
            var chatMessages = _chatMessageRepository.Fetch(x => x.ChatId == id).OrderBy(x => x.DateTime).ToList();
            var objectRequest = _objectRequestRepository.Fetch(x => x.AggregateId == chat.ObjectRequestId).Single();

            var chatMessageViewModels = chatMessages.Select(x => new ChatMessageViewModel {
                DateTime = x.DateTime,
                UserId = x.UserId,
                Message = x.Message
            }).ToList();

            var isForBlockedObjectRequest = objectRequest.Status == ObjectRequestStatus.BlockedForForbiddenWords.ToString() || objectRequest.Status == ObjectRequestStatus.BlockedByAdmin.ToString();
            if (isForBlockedObjectRequest) _orchardServices.Notifier.Add(NotifyType.Warning, T("This request is blocked. It is currently not possible to add new messages."));

            var isStopped = objectRequest.Status == ObjectRequestStatus.Stopped.ToString();
            if (isStopped) _orchardServices.Notifier.Add(NotifyType.Warning, T("This request has been stopped. It is currently not possible to add new messages."));

            var chatViewModel = new ChatViewModel {
                Messages = chatMessageViewModels,
                ChatId = id,
                ObjectDescription = objectRequest.Description,
                RequestingUserName = chat.RequestingUserName,
                ConfirmingUserName = chat.ConfirmingUserName,
                RequestingUserId = chat.RequestingUserId,
                IsForBlockedObjectRequest = isForBlockedObjectRequest,
                IsStopped = isStopped
            };
            return chatViewModel;
        }
    }
}