using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Title.Models;
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
        private readonly IContentManager _contentManager;
        //private readonly IRepository<ArchetypePartRecord> _archetypeRepository;
        //private readonly IRepository<ArchetypedSynonymRecord> _synonymsRepository;
        //private readonly ICommandHandler<CreateArchetype> _createArchetypeCommandHandler;
        //private readonly ICommandHandler<SetSynonymArchetypes> _setSynonymArchetypesCommandHandler;

        public ArchetypesController(
            IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            var records = _contentManager.Query("Archetype").List();
            return View(records);
        }

        public ActionResult Synonyms()
        {
            var synonyms = _contentManager
                .Query("Synonym")
                .List();

            var archetypes = _contentManager
                .Query("Archetype")
                .List()
                .ToList();

            var viewModel = new SynonymsViewModel {
                Synonyms = synonyms.OrderBy(x => x.As<TitlePart>().Title).Select(x => new EditArchetypedSynonymViewModel {
                    Synonym = x.As<TitlePart>().Title,
                    SelectedArchetypeId = ((ContentPickerField)((ContentPart) x.Content.Synonym).Get(typeof(ContentPickerField), "Archetype")).Ids.FirstOrDefault(),
                    Archetypes = archetypes
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Synonyms(SynonymsViewModel viewModel) {
            var synonyms = _contentManager
                .Query("Synonym")
                .List();

            foreach (var editArchetypedSynonymViewModel in viewModel.Synonyms) {
                var synonym = synonyms.FirstOrDefault(x => x.As<TitlePart>().Title == editArchetypedSynonymViewModel.Synonym);
                if (synonym == null) {
                    continue;
                }

                var field = (ContentPickerField)((ContentPart)synonym.Content.Synonym).Get(typeof(ContentPickerField), "Archetype");
                if (editArchetypedSynonymViewModel.SelectedArchetypeId.HasValue) {
                    field.Ids = new[] {editArchetypedSynonymViewModel.SelectedArchetypeId.Value};
                }
                else {
                    field.Ids = new int[0];
                }
            }

            return RedirectToAction("Synonyms");
        }

        public Localizer T { get; set; }
    }
}