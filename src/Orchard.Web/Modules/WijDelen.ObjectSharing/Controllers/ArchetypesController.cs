using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.UI.Admin;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ArchetypesController : Controller {
        private readonly IFindAllArchetypesQuery _findAllArchetypesQuery;
        private readonly IFindAllSynonymsQuery _findAllSynonymsQuery;

        public ArchetypesController(IFindAllArchetypesQuery findAllArchetypesQuery, IFindAllSynonymsQuery findAllSynonymsQuery) {
            _findAllArchetypesQuery = findAllArchetypesQuery;
            _findAllSynonymsQuery = findAllSynonymsQuery;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            var records = _findAllArchetypesQuery.GetResult();
            return View(records);
        }

        public ActionResult Synonyms()
        {
            var synonyms = _findAllSynonymsQuery.GetResult();

            var archetypes = _findAllArchetypesQuery.GetResult().ToList();

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
            var synonyms = _findAllSynonymsQuery.GetResult();

            foreach (var editArchetypedSynonymViewModel in viewModel.Synonyms) {
                var synonym = synonyms.FirstOrDefault(x => x.As<TitlePart>().Title == editArchetypedSynonymViewModel.Synonym);
                if (synonym == null) {
                    continue;
                }

                var field = (ContentPickerField)((ContentPart)synonym.Content.Synonym).Get(typeof(ContentPickerField), "Archetype");
                if (editArchetypedSynonymViewModel.SelectedArchetypeId.HasValue) {
                    field.Ids = new[] {editArchetypedSynonymViewModel.SelectedArchetypeId.Value};
                } else {
                    field.Ids = new int[0];
                }
            }

            return RedirectToAction("Synonyms");
        }

        public Localizer T { get; set; }
    }
}