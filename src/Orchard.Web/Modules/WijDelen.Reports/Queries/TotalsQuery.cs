using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Users.Models;
using WijDelen.ObjectSharing.Models;
using WijDelen.Reports.Models;

namespace WijDelen.Reports.Queries {
    public class TotalsQuery : ITotalsQuery {
        private readonly IRepository<UserPartRecord> _userRepository;
        private readonly IRepository<ObjectRequestRecord> _requestRepository;
        private readonly IContentManager _contentManager;

        public TotalsQuery(IRepository<UserPartRecord> userRepository, IRepository<ObjectRequestRecord> requestRepository, IContentManager contentManager) {
            _userRepository = userRepository;
            _requestRepository = requestRepository;
            _contentManager = contentManager;
        }

        public Totals GetResults() {
            var result = new Totals();

            result.Users = _userRepository.Count(x => true);
            result.ObjectRequests = _requestRepository.Count(x => true);
            result.Groups = _contentManager.HqlQuery().ForType("Group").Count();

            return result;
        }
    }
}