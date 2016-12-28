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
        private readonly IRepository<ReceivedObjectRequestRecord> _repository;
        private readonly IOrchardServices _orchardServices;

        public ReceivedObjectRequestController(IRepository<ReceivedObjectRequestRecord> repository, IOrchardServices orchardServices) {
            _repository = repository;
            _orchardServices = orchardServices;
        }

        [Authorize]
        public ActionResult Index()
        {
            var records = _repository.Fetch(x => x.UserId == _orchardServices.WorkContext.CurrentUser.Id).OrderByDescending(x => x.ReceivedDateTime).ToList();
            return View(records);
        }

        public Localizer T { get; set; }
    }
}