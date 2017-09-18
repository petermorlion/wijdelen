using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Themes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.ViewModels.Feed;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    [Authorize]
    public class FeedController : Controller {
        private readonly IFindFeedViewModelsQuery _findFeedViewModelsQuery;
        private readonly IOrchardServices _orchardServices;
        private const int FeedLimit = 20;

        public Localizer T { get; set; }

        public FeedController(IFindFeedViewModelsQuery findFeedViewModelsQuery, IOrchardServices orchardServices) {
            _findFeedViewModelsQuery = findFeedViewModelsQuery;
            _orchardServices = orchardServices;
        }

        public ActionResult Index() {
            var groupId = _orchardServices.WorkContext.CurrentUser.As<GroupMembershipPart>().Group.Id;
            var currentUserId = _orchardServices.WorkContext.CurrentUser.Id;
            var feedItems = _findFeedViewModelsQuery.GetResults(groupId, currentUserId, FeedLimit);
            var model = new IndexViewModel { Items = feedItems };
            return View(model);
        }
    }
}