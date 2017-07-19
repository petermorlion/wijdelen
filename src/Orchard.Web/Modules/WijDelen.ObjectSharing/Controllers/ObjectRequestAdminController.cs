using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels.Admin;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ObjectRequestAdminController : Controller {
        private readonly IRepository<ObjectRequestRecord> _objectRequestRecordRepository;
        private readonly ICommandHandler<UnblockObjectRequests> _unblockObjectRequestCommandHandler;
        private readonly INotifier _notifier;

        public ObjectRequestAdminController(IRepository<ObjectRequestRecord> objectRequestRecordRepository, ICommandHandler<UnblockObjectRequests> unblockObjectRequestCommandHandler, INotifier notifier) {
            _objectRequestRecordRepository = objectRequestRecordRepository;
            _unblockObjectRequestCommandHandler = unblockObjectRequestCommandHandler;
            _notifier = notifier;
        }

        public Localizer T { get; set; }

        public ActionResult Index(int page = 1, int selectedObjectRequestStatusValue = -1) {
            const int take = 50;

            var filteredRecords = _objectRequestRecordRepository
                .Table;

            if (selectedObjectRequestStatusValue != -1) {
                var objectRequestStatus = (ObjectRequestStatus)selectedObjectRequestStatusValue;
                filteredRecords = filteredRecords.Where(x => x.Status == objectRequestStatus.ToString());
            }

            var count = filteredRecords.Count();
            var totalPages = GetTotalPages(count, take);

            if (page > totalPages) {
                return RedirectToAction("Index", new { page = totalPages });
            }

            var skip = (page - 1) * take;

            var records = filteredRecords
                .OrderByDescending(x => x.CreatedDateTime)
                .Skip(skip)
                .Take(take)
                .ToList()
                .Select(x => new ObjectRequestRecordViewModel {
                    Id = x.Id,
                    AggregateId = x.AggregateId,
                    IsSelected = false,
                    GroupName = x.GroupName,
                    Description = x.Description,
                    Status = GetStatus(x),
                    CreatedDateTime = x.CreatedDateTime.ToLocalTime()
                })
                .ToList();

            var hasNextPage = page * take < count;
            var hasPreviousPage = page > 1;

            var possibleStatusses = new List<ObjectRequestStatusViewModel> {
                new ObjectRequestStatusViewModel {ObjectRequestStatusValue = -1, Translation = ""},
                new ObjectRequestStatusViewModel {ObjectRequestStatusValue = (int) ObjectRequestStatus.None, Translation = T("OK").ToString()},
                new ObjectRequestStatusViewModel {ObjectRequestStatusValue = (int) ObjectRequestStatus.BlockedForForbiddenWords, Translation = T("Blocked (forbidden words)").ToString()}
            };

            var viewModel = new ObjectRequestAdminViewModel {
                ObjectRequests = records,
                Page = page,
                ObjectRequestsCount = count,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage,
                TotalPages = totalPages,
                PossibleStatusses = possibleStatusses
            };

            return View(viewModel);
        }

        /// <summary>
        /// Returns the index of the last page. Implemented using a calculation found here: http://stackoverflow.com/questions/17944/how-to-round-up-the-result-of-integer-division
        /// </summary>
        /// <param name="totalRecords"></param>
        /// <param name="recordsPerPage"></param>
        /// <returns></returns>
        private static int GetTotalPages(int totalRecords, int recordsPerPage) {
            var count = (totalRecords + recordsPerPage - 1) / recordsPerPage;
            return count;
        }

        [HttpPost]
        [Orchard.Mvc.FormValueRequired("submit.Unblock")]
        public ActionResult Index(ObjectRequestAdminViewModel viewModel) {
            if (viewModel.ObjectRequests.All(x => !x.IsSelected)) {
                _notifier.Add(NotifyType.Warning, T("Please select at least one request to unblock."));  
            } else {
                var aggregateIds = viewModel.ObjectRequests.Where(x => x.IsSelected).Select(x => x.AggregateId).ToList();
                var command = new UnblockObjectRequests(aggregateIds);
                _unblockObjectRequestCommandHandler.Handle(command);
                _notifier.Add(NotifyType.Success, T("The selected requests were unblocked and mails have been sent to the users."));
            }
            
            return RedirectToAction("Index", new {page = viewModel.Page});
        }

        [HttpPost]
        [Orchard.Mvc.FormValueRequired("submit.Filter")]
        [ActionName("Index")]
        public ActionResult IndexPost(int selectedObjectRequestStatusValue = -1)
        {
            if (selectedObjectRequestStatusValue == -1) {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index", new { selectedObjectRequestStatusValue });
        }

        private string GetStatus(ObjectRequestRecord objectRequestRecord) {
            if (objectRequestRecord.Status == ObjectRequestStatus.BlockedForForbiddenWords.ToString()) {
                return T("Blocked (forbidden words)").ToString();
            }

            if (objectRequestRecord.Status == ObjectRequestStatus.None.ToString()) {
                return T("OK").ToString();
            }

            if (objectRequestRecord.Status == ObjectRequestStatus.BlockedByAdmin.ToString()) {
                return T("Blocked (by administrator)").ToString();
            }

            return objectRequestRecord.Status;
        }
    }
}