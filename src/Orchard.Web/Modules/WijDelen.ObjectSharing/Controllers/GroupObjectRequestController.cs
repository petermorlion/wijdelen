using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Themes;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    [Authorize]
    public class GroupObjectRequestController : Controller {
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly IFindUsersByIdsQuery _findUsersByIdsQuery;

        public GroupObjectRequestController(IRepository<ObjectRequestRecord> objectRequestRepository, IOrchardServices orchardServices, IFindUsersByIdsQuery findUsersByIdsQuery) {
            _objectRequestRepository = objectRequestRepository;
            _orchardServices = orchardServices;
            _findUsersByIdsQuery = findUsersByIdsQuery;
        }

        public ActionResult Index() {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            var currentGroupId = currentUser.As<GroupMembershipPart>()?.Group?.Id;
            if (!currentGroupId.HasValue) {
                return PartialView();
            }

            var objectRequests = _objectRequestRepository
                .Fetch(x => x.GroupId == currentGroupId.Value && x.UserId != currentUser.Id && x.Status == ObjectRequestStatus.None.ToString())
                .Take(3)
                .ToList();
            var userIds = objectRequests.Select(x => x.UserId).Distinct().ToArray();
            var users = _findUsersByIdsQuery.GetResult(userIds).ToList();

            var viewModels = (from objectRequestRecord in objectRequests
                let user = users.FirstOrDefault(x => x.Id == objectRequestRecord.UserId)
                where user != null
                select new GroupObjectRequestViewModel {
                    Description = objectRequestRecord.Description,
                    UserName = user.UserName,
                    FirstName = user.As<UserDetailsPart>()?.FirstName,
                    LastName = user.As<UserDetailsPart>()?.LastName
                }).ToList();

            return PartialView(viewModels);
        }
    }
}