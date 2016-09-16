using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ArchetypesController : Controller {
        private readonly IRepository<ItemArchetypeRecord> _archetypeRepository;
        private readonly IRepository<ArchetypedSynonymRecord> _synonymsRepository;

        public ArchetypesController(IRepository<ItemArchetypeRecord> archetypeRepository, IRepository<ArchetypedSynonymRecord> synonymsRepository) {
            _archetypeRepository = archetypeRepository;
            _synonymsRepository = synonymsRepository;

            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            var records = _archetypeRepository.Table.ToList();
            return View(records);
        }

        public ActionResult Synonyms() {
            var synonymRecords = _synonymsRepository.Table.ToList();
            var archetypeRecords = _archetypeRepository.Table.ToList();
            var viewModel = new SynonymsViewModel(synonymRecords, archetypeRecords);
            return View(viewModel);
        }

        public Localizer T { get; set; }
    }
}