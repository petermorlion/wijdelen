using System.Linq;
using System.Web.Http;
using Orchard.Core.Title.Models;
using WijDelen.ObjectSharing.Infrastructure.Queries;

namespace WijDelen.ObjectSharing.Controllers.Api {
    public class ArchetypesController : ApiController {
        private readonly ISearchArchetypesByTitleQuery _query;

        public ArchetypesController(ISearchArchetypesByTitleQuery query) {
            _query = query;
        }

        public IHttpActionResult Get(string input) {
            var archetypes = _query.GetResult(input);
            
            var archetypeMatches = archetypes
                .Select(x => x.Parts.OfType<TitlePart>().Single().Title)
                .ToList();

            return Ok(archetypeMatches);
        }
    }
}