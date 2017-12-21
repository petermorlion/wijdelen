using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using WijDelen.Mobile;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    [Authorize]
    public class FeedController : Controller {
        private readonly IRepository<FeedItemRecord> _feedItemRepository;
        private readonly IOrchardServices _orchardServices;
        private const int FeedLimit = 20;

        public Localizer T { get; set; }

        public FeedController(IRepository<FeedItemRecord> feedItemRepository, IOrchardServices orchardServices) {
            _feedItemRepository = feedItemRepository;
            _orchardServices = orchardServices;
        }

        public ActionResult Index() {
            var currentUserId = _orchardServices.WorkContext.CurrentUser.Id;
            var feedItems = _feedItemRepository.Fetch(x => x.UserId == currentUserId).OrderByDescending(x => x.DateTime).Take(FeedLimit).ToList();
            var model = new FeedViewModel { Items = feedItems };
            return this.ViewOrJson(model);
        }
    }
}