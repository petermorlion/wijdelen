using Orchard.Data;
using WijDelen.ObjectSharing.Models;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    public class MonthSummaryQuery : IMonthSummaryQuery {
        private readonly IRepository<ObjectRequestRecord> _requestRepository;
        private readonly IRepository<ObjectRequestMailRecord> _mailRepository;

        public MonthSummaryQuery(IRepository<ObjectRequestRecord> requestRepository, IRepository<ObjectRequestMailRecord> mailRepository) {
            _requestRepository = requestRepository;
            _mailRepository = mailRepository;
        }
        public SummaryViewModel GetResults(int monthNumber) {
            var results = new SummaryViewModel();

            results.MailCount = _mailRepository.Count(x => x.SentDateTime.Month == monthNumber);
            results.ObjectRequestCount = _requestRepository.Count(x => x.CreatedDateTime.Month == monthNumber);

            return results;
        }
    }
}