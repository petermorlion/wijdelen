using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Core.Contents.Controllers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
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

        public ObjectRequestAdminController(IRepository<ObjectRequestRecord> objectRequestRecordRepository, ICommandHandler<UnblockObjectRequests> unblockObjectRequestCommandHandler) {
            _objectRequestRecordRepository = objectRequestRecordRepository;
            _unblockObjectRequestCommandHandler = unblockObjectRequestCommandHandler;
        }

        public Localizer T { get; set; }

        public ActionResult Index(int page = 1) {
            var take = 50;
            var skip = (page - 1) * take;

            var records = _objectRequestRecordRepository.Table
                .OrderBy(x => x.CreatedDateTime)
                .Skip(skip)
                .Take(take)
                .ToList()
                .Select(x => new ObjectRequestRecordViewModel {
                    AggregateId = x.AggregateId,
                    IsSelected = false,
                    GroupName = x.GroupName,
                    Description = x.Description,
                    Status = GetStatus(x)
                })
                .ToList();

            var count = _objectRequestRecordRepository.Table.Count();

            var hasNextPage = page * take < count;
            var hasPreviousPage = page > 1;

            var viewModel = new ObjectRequestAdminViewModel {
                ObjectRequests = records,
                Page = page,
                ObjectRequestsCount = count,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage
            };

            return View(viewModel);
        }

        [HttpPost]
        [FormValueRequired("submit.Unblock")]
        public ActionResult Index(List<ObjectRequestRecordViewModel> viewModels) {
            var aggregateIds = viewModels.Where(x => x.IsSelected).Select(x => x.AggregateId).ToList();
            var command = new UnblockObjectRequests(aggregateIds);
            _unblockObjectRequestCommandHandler.Handle(command);
            return RedirectToAction("Index");
        }

        private string GetStatus(ObjectRequestRecord objectRequestRecord) {
            if (objectRequestRecord.Status == ObjectRequestStatus.BlockedForForbiddenWords.ToString()) {
                return T("Blocked").ToString();
            }

            if (objectRequestRecord.Status == ObjectRequestStatus.None.ToString()) {
                return "";
            }

            return objectRequestRecord.Status;
        }
    }
}