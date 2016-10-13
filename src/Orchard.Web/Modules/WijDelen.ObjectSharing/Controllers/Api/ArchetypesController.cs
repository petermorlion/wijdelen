using System.Linq;
using System.Web.Http;
using Orchard.Data;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers.Api {
    public class ArchetypesController : ApiController {
        //private readonly IRepository<ArchetypePartRecord> _archetypeRepository;
        //private readonly IRepository<ArchetypedSynonymRecord> _synonymsRepository;

        //public ArchetypesController(IRepository<ArchetypePartRecord> archetypeRepository, IRepository<ArchetypedSynonymRecord> synonymsRepository) {
        //    _archetypeRepository = archetypeRepository;
        //    _synonymsRepository = synonymsRepository;
        //}
        
        //public IHttpActionResult Get(string input) {
        //    var archetypeMatches = _archetypeRepository.Fetch(x => x.Name.ToLowerInvariant().Contains(input.ToLowerInvariant())).Select(x => x.Name);
        //    var synonymMatches = _synonymsRepository.Fetch(x => x.Synonym.ToLowerInvariant().Contains(input.ToLowerInvariant())).Select(x => x.Archetype);

        //    var result = archetypeMatches.Union(synonymMatches).ToList();
        //    return Ok(result);
        //}
    }
}