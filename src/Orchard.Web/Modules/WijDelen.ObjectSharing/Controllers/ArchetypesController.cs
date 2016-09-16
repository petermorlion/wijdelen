using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ArchetypesController : Controller {
        private readonly IRepository<ArchetypeRecord> _archetypeRepository;
        private readonly IRepository<ArchetypedSynonymRecord> _synonymsRepository;
        private readonly ICommandHandler<CreateArchetype> _createArchetypeCommandHandler;

        public ArchetypesController(
            IRepository<ArchetypeRecord> archetypeRepository, 
            IRepository<ArchetypedSynonymRecord> synonymsRepository,
            ICommandHandler<CreateArchetype> createArchetypeCommandHandler) {
            _archetypeRepository = archetypeRepository;
            _synonymsRepository = synonymsRepository;
            _createArchetypeCommandHandler = createArchetypeCommandHandler;

            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            var records = _archetypeRepository.Table.ToList();
            return View(records);
        }

        public ActionResult Synonyms() {
            var synonymRecords = _synonymsRepository.Table.ToList();
            var archetypeRecords = _archetypeRepository.Table.ToList();
            var orderedArchetypes = archetypeRecords.OrderBy(x => x.Name).ToList();
            var viewModel = new SynonymsViewModel {
                Synonyms = synonymRecords.OrderBy(x => x.Synonym).Select(x => new EditArchetypedSynonymViewModel {
                    Synonym = x.Synonym,
                    Archetypes = orderedArchetypes.Select(a => new ArchetypeViewModel { Id = a.AggregateId, Name = a.Name }).ToList(),
                    SelectedArchetypeId = orderedArchetypes.SingleOrDefault(a => a.Name == x.Archetype)?.AggregateId.ToString()
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Synonyms(SynonymsViewModel viewModel) {
            return RedirectToAction("Synonyms");
        }

        public ActionResult Create() {
            return View(new CreateArchetypeViewModel());
        }

        [HttpPost]
        public ActionResult Create(CreateArchetypeViewModel viewModel) {
            if (string.IsNullOrWhiteSpace(viewModel.Name)) {
                ModelState.AddModelError<CreateArchetypeViewModel, string>(m => m.Name, T("Please provide a name for the archetype."));
                return View(viewModel);
            }

            var command = new CreateArchetype(viewModel.Name);
            _createArchetypeCommandHandler.Handle(command);
            return RedirectToAction("Index");
        }

        public Localizer T { get; set; }
    }
}