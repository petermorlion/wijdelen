using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    public class ReceivedObjectRequestController : Controller {
        private readonly IRepository<ObjectRequestMailRecord> _objectRequestMailRepository;
        private readonly IOrchardServices _orchardServices;

        public ReceivedObjectRequestController(IRepository<ObjectRequestMailRecord> objectRequestMailRepository, IOrchardServices orchardServices) {
            _objectRequestMailRepository = objectRequestMailRepository;
            _orchardServices = orchardServices;
        }

        [Authorize]
        public ActionResult Index()
        {
            var records = _objectRequestMailRepository.Fetch(x => x.ReceivingUserId == _orchardServices.WorkContext.CurrentUser.Id).OrderByDescending(x => x.SentDateTime).ToList();
            return View(records);
        }

        public Localizer T { get; set; }
    }
}