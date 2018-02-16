using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.ObjectSharing.Models;
using WijDelen.Reports.ViewModels;

namespace WijDelen.Reports.Queries {
    public class MonthSummaryQuery : IMonthSummaryQuery {
        private readonly IRepository<ObjectRequestRecord> _requestRepository;
        private readonly IRepository<ObjectRequestNotificationRecord> _notificationRepository;
        private readonly IRepository<ObjectRequestResponseRecord> _responseRepository;

        public MonthSummaryQuery(
            IRepository<ObjectRequestRecord> requestRepository, 
            IRepository<ObjectRequestNotificationRecord> notificationRepository, 
            IRepository<ObjectRequestResponseRecord> responseRepository) {
            _requestRepository = requestRepository;
            _notificationRepository = notificationRepository;
            _responseRepository = responseRepository;
        }
        public SummaryViewModel GetResults(int yearNumber, int monthNumber) {
            var results = new SummaryViewModel();

            results.MailCount = _notificationRepository.Count(x => x.SentDateTime.Month == monthNumber && x.SentDateTime.Year == yearNumber);
            results.ObjectRequestCount = _requestRepository.Count(x => x.CreatedDateTime.Month == monthNumber && x.CreatedDateTime.Year == yearNumber);
            results.YesCount = _responseRepository.Count(x => x.DateTimeResponded.Month == monthNumber && x.DateTimeResponded.Year == yearNumber && x.Response == ObjectRequestAnswer.Yes);
            results.NoCount = _responseRepository.Count(x => x.DateTimeResponded.Month == monthNumber && x.DateTimeResponded.Year == yearNumber && x.Response == ObjectRequestAnswer.No);
            results.NotNowCount = _responseRepository.Count(x => x.DateTimeResponded.Month == monthNumber && x.DateTimeResponded.Year == yearNumber && x.Response == ObjectRequestAnswer.NotNow);

            return results;
        }
    }
}