using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Data;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels.Admin;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ObjectRequestDetailsAdminController : Controller {
        private readonly IGetUserByIdQuery _getUserByIdQuery;
        private readonly INotifier _notifier;
        private readonly IRepository<ObjectRequestRecord> _objectRequestRecordRepository;

        public ObjectRequestDetailsAdminController(IRepository<ObjectRequestRecord> repository, IGetUserByIdQuery getUserByIdQuery, INotifier notifier) {
            _objectRequestRecordRepository = repository;
            _getUserByIdQuery = getUserByIdQuery;
            _notifier = notifier;
        }

        public Localizer T { get; set; }

        public ViewResult Index(Guid objectRequestId) {
            var objectRequestRecord = _objectRequestRecordRepository.Table.Single(x => x.AggregateId == objectRequestId);
            var user = _getUserByIdQuery.GetResult(objectRequestRecord.UserId);
            var viewModel = new ObjectRequestDetailsAdminViewModel {
                Status = GetStatus(objectRequestRecord),
                CreatedDateTime = objectRequestRecord.CreatedDateTime.ToLocalTime(),
                Description = objectRequestRecord.Description,
                ExtraInfo = objectRequestRecord.ExtraInfo,
                GroupName = objectRequestRecord.GroupName,
                FirstName = user.As<UserDetailsPart>().FirstName,
                LastName = user.As<UserDetailsPart>().LastName,
                ForbiddenWords = ForbiddenWords.GetForbiddenWordsInString(objectRequestRecord.Description).Union(ForbiddenWords.GetForbiddenWordsInString(objectRequestRecord.ExtraInfo)).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(Guid objectRequestId, string blockReason)
        {
            _notifier.Add(NotifyType.Success, T("The request was blocked and a mail was sent to the user."));
            return RedirectToAction("Index", new {objectRequestId});
        }

        private string GetStatus(ObjectRequestRecord objectRequestRecord) {
            if (objectRequestRecord.Status == ObjectRequestStatus.BlockedForForbiddenWords.ToString())
                return T("Blocked (forbidden words)").ToString();

            if (objectRequestRecord.Status == ObjectRequestStatus.None.ToString())
                return T("OK").ToString();

            return objectRequestRecord.Status;
        }
    }
}