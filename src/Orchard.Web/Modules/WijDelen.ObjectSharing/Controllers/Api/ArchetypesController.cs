using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Orchard.Data;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers.Api {
    public class ArchetypesController : ApiController {
        private readonly IRepository<ArchetypeRecord> _archetypeRepository;
        private readonly IRepository<ArchetypedSynonymRecord> _synonymsRepository;

        public ArchetypesController(IRepository<ArchetypeRecord> archetypeRepository, IRepository<ArchetypedSynonymRecord> synonymsRepository) {
            _archetypeRepository = archetypeRepository;
            _synonymsRepository = synonymsRepository;
        }
        
        public IHttpActionResult Get(string input) {
            var matches = _archetypeRepository.Fetch(x => x.Name.ToLowerInvariant().Contains(input.ToLowerInvariant())).ToList();

            if (matches.Any()) {
                var result = matches.Select(x => x.Name).ToList();
                return Ok(result);
            }

            var itemsList = new List<string>{
                "Item 1",
                "Item 2",
                "Item 3",
                input
            };

            return Ok(itemsList);
        }
    }
}