using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels.Feed;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    [Authorize]
    public class FeedController : Controller {
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly IFindUsersByIdsQuery _findUsersByIdsQuery;
        private const int FeedLimit = 10;

        public Localizer T { get; set; }

        public FeedController(IRepository<ObjectRequestRecord> objectRequestRepository, IOrchardServices orchardServices, IFindUsersByIdsQuery findUsersByIdsQuery) {
            _objectRequestRepository = objectRequestRepository;
            _orchardServices = orchardServices;
            _findUsersByIdsQuery = findUsersByIdsQuery;
        }

        public ActionResult Index() {
            var groupId = _orchardServices.WorkContext.CurrentUser.As<GroupMembershipPart>().Group.Id;
            var objectRequests = _objectRequestRepository.Fetch(x => x.GroupId == groupId).OrderByDescending(x => x.CreatedDateTime).Take(FeedLimit).ToList();
            var userIds = objectRequests.Select(x => x.UserId).Distinct().ToList();
            var users = _findUsersByIdsQuery.GetResult(userIds.ToArray());

            var model = new IndexViewModel {ObjectRequests = new List<ObjectRequestViewModel>()};

            foreach (var objectRequestRecord in objectRequests) {
                var user = users.SingleOrDefault(x => x.Id == objectRequestRecord.UserId);
                var userDetailsPart = user?.As<UserDetailsPart>();
                model.ObjectRequests.Add(new ObjectRequestViewModel {
                    CreatedDateTime = objectRequestRecord.CreatedDateTime.ToLocalTime(),
                    Description = objectRequestRecord.Description,
                    FirstName = userDetailsPart.FirstName,
                    LastName = userDetailsPart?.LastName
                });
            }

            return View(model);
        }
    }
}