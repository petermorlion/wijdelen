using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    [Authorize]
    public class ReceivedObjectRequestController : Controller {
        private readonly IRepository<ReceivedObjectRequestRecord> _repository;
        private readonly IFindUsersByIdsQuery _usersQuery;
        private readonly IOrchardServices _orchardServices;

        public ReceivedObjectRequestController(IRepository<ReceivedObjectRequestRecord> repository, IFindUsersByIdsQuery usersQuery, IOrchardServices orchardServices) {
            _repository = repository;
            _usersQuery = usersQuery;
            _orchardServices = orchardServices;
        }

        public ActionResult Index()
        {
            var records = _repository.Fetch(x => x.UserId == _orchardServices.WorkContext.CurrentUser.Id).OrderByDescending(x => x.ReceivedDateTime).ToList();
            var users = _usersQuery.GetResult(records.Select(x => x.RequestingUserId).Distinct().ToArray()).ToList();

            var viewModels = new List<ReceivedObjectRequestViewModel>();
            foreach (var receivedObjectRequestRecord in records) {
                var user = users.SingleOrDefault(u => u.Id == receivedObjectRequestRecord.RequestingUserId);
                if (user == null)
                    continue;

                viewModels.Add(new ReceivedObjectRequestViewModel {
                    ObjectRequestId = receivedObjectRequestRecord.ObjectRequestId,
                    Description = receivedObjectRequestRecord.Description,
                    ExtraInfo = receivedObjectRequestRecord.ExtraInfo,
                    UserName = user.GetUserDisplayName(),
                    ReceivedDateTime = receivedObjectRequestRecord.ReceivedDateTime
                });
            }

            return View(viewModels);
        }

        public Localizer T { get; set; }
    }
}