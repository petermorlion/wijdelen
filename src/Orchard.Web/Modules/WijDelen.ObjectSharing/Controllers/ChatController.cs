using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    public class ChatController : Controller {
        private readonly IRepository<ChatMessageRecord> _chatMessageRepository;
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly ICommandHandler<StartChat> _startChatCommandHandler;
        private readonly ICommandHandler<AddChatMessage> _addChatMessageCommandHandler;
        private readonly IRepository<ChatRecord> _chatRepository;
        private readonly IOrchardServices _orchardServices;

        public ChatController(
            IRepository<ChatMessageRecord> chatMessageRepository, 
            IRepository<ObjectRequestRecord> objectRequestRepository,
            ICommandHandler<StartChat> startChatCommandHandler,
            ICommandHandler<AddChatMessage> addChatMessageCommandHandler,
            IRepository<ChatRecord> chatRepository,
            IOrchardServices orchardServices) {
            _chatMessageRepository = chatMessageRepository;
            _objectRequestRepository = objectRequestRepository;
            _startChatCommandHandler = startChatCommandHandler;
            _addChatMessageCommandHandler = addChatMessageCommandHandler;
            _chatRepository = chatRepository;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        [Authorize]
        public ActionResult Start(Guid objectRequestId) {
            var objectRequestRecord = _objectRequestRepository.Fetch(x => x.AggregateId == objectRequestId).SingleOrDefault();

            if (objectRequestRecord == null) {
                return new HttpNotFoundResult();
            }

            var startChat = new StartChat(objectRequestId, objectRequestRecord.UserId, _orchardServices.WorkContext.CurrentUser.Id);

            _startChatCommandHandler.Handle(startChat);

            return RedirectToAction("Index", new {chatId = startChat.ChatId});
        }

        [Authorize]
        public ActionResult Index(Guid chatId) {
            var chatMessages = _chatMessageRepository.Fetch(x => x.ChatId == chatId).OrderBy(x => x.DateTime).ToList();
            if (!chatMessages.Any()) {
                return new HttpNotFoundResult();
            }

            var chat = _chatRepository.Fetch(x => x.ChatId == chatId).Single();
            var objectRequest = _objectRequestRepository.Fetch(x => x.AggregateId == chat.ObjectRequestId).Single();

            var chatMessageViewModels = chatMessages.Select(x => new ChatMessageViewModel {
                DateTime = x.DateTime,
                UserName = x.UserName,
                Message = x.Message
            }).ToList();

            return View(new ChatViewModel {
                Messages = chatMessageViewModels,
                ChatId = chatId,
                ObjectDescription = objectRequest.Description
            });
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddMessage(ChatViewModel viewModel) {
            if (string.IsNullOrWhiteSpace(viewModel.NewMessage))
            {
                ModelState.AddModelError<ChatViewModel, string>(m => m.NewMessage, T("Please provide a message."));
            }
            
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var userId = _orchardServices.WorkContext.CurrentUser.Id;
            var chat = _chatRepository.Fetch(x => x.ChatId == viewModel.ChatId).Single();

            if (chat.ConfirmingUserId != userId && chat.RequestingUserId != userId) {
                return new HttpUnauthorizedResult();
            }

            var addChatMessage = new AddChatMessage(viewModel.ChatId, userId, viewModel.NewMessage, DateTime.UtcNow);
            _addChatMessageCommandHandler.Handle(addChatMessage);
            return RedirectToAction("Index", new {chatId = viewModel.ChatId});
        }

        public Localizer T { get; set; }
    }
}