using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ArchetypesController : Controller {
        private readonly IRepository<ItemArchetypeRecord> _archetypeRepository;
        private readonly IRepository<UnarchetypedSynonymRecord> _synonymsRepository;

        public ArchetypesController(IRepository<ItemArchetypeRecord> archetypeRepository, IRepository<UnarchetypedSynonymRecord> synonymsRepository) {
            _archetypeRepository = archetypeRepository;
            _synonymsRepository = synonymsRepository;

            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            var records = _archetypeRepository.Table.ToList();
            return View(records);
        }

        public ActionResult Unarchetyped() {
            var records = _synonymsRepository.Table.ToList();
            return View(records);
        }

        public Localizer T { get; set; }
    }
}