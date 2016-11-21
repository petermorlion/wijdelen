using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Themes;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    public class ChatController : Controller {
        private readonly IRepository<ChatMessageRecord> _repository;

        public ChatController(IRepository<ChatMessageRecord> repository) {
            _repository = repository;
        }

        [Authorize]
        public ActionResult Index(Guid chatId) {
            var chatMessages = _repository.Fetch(x => x.ChatId == chatId).OrderBy(x => x.DateTime).ToList();
            var chatMessageViewModels = chatMessages.Select(x => new ChatMessageViewModel {
                DateTime = x.DateTime,
                UserName = x.UserName,
                Message = x.Message
            }).ToList();

            return View(new ChatViewModel {
                Messages = chatMessageViewModels
            });
        }
    }
}