using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ArchetypesController : Controller {
        private readonly IRepository<ItemArchetypeRecord> _repository;

        public ArchetypesController(IRepository<ItemArchetypeRecord> repository) {
            _repository = repository;

            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            var records = _repository.Table.ToList();
            return View(records);
        }

        public ActionResult Unarchetyped() {
            return View();
        }

        public Localizer T { get; set; }
    }
}