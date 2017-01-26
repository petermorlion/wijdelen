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
        //private readonly ITransactionManager _transactionManager;

        public TotalsQuery(IRepository<UserPartRecord> userRepository, IRepository<ObjectRequestRecord> requestRepository, IContentManager contentManager) {
            _userRepository = userRepository;
            _requestRepository = requestRepository;
            _contentManager = contentManager;
            //_transactionManager = transactionManager;
        }

        public Totals GetResults() {
            var result = new Totals();

            result.Users = _userRepository.Count(x => true);
            result.ObjectRequests = _requestRepository.Count(x => true);
            result.Groups = _contentManager.HqlQuery().ForType("Group").Count();
            
            //session.CreateSQLQuery($"SELECT 'ObjectRequests' AS 'Type', COUNT(*) FROM {typeof(ObjectRequestRecord)} " +
            //                       "UNION ALL " +
            //                       "SELECT 'Users' AS 'Type', COUNT(*) FROM Orchard_Users_UserPartRecord " +
            //                       "UNION ALL " +
            //                       "SELECT 'Groups' AS 'Type', COUNT(*) FROM Orchard_Framework_ContentTypeRecord t " +
            //                       "    INNER JOIN Orchard_Framework_ContentItemRecord i ON i.ContentType_id = t.Id WHERE t.Name = 'Group'");
            //var list = session.List();

            return result;
        }
    }
}