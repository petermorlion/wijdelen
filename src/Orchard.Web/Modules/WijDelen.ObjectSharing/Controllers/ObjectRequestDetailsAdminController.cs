using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.ViewModels.Admin;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ObjectRequestDetailsAdminController : Controller
    {
        private readonly IRepository<ObjectRequestRecord> _objectRequestRecordRepository;
        private readonly IGetUserByIdQuery _getUserByIdQuery;

        public ObjectRequestDetailsAdminController(IRepository<ObjectRequestRecord> repository, IGetUserByIdQuery getUserByIdQuery) {
            _objectRequestRecordRepository = repository;
            _getUserByIdQuery = getUserByIdQuery;
        }

        public Localizer T { get; set; }

        public ViewResult Index(int objectRequestId) {
            var objectRequestRecord = _objectRequestRecordRepository.Get(objectRequestId);
            var user = _getUserByIdQuery.GetResult(objectRequestRecord.UserId);
            var viewModel = new ObjectRequestDetailsAdminViewModel {
                Status = objectRequestRecord.Status,
                CreatedDateTime = objectRequestRecord.CreatedDateTime.ToLocalTime(),
                Description = objectRequestRecord.Description,
                ExtraInfo = objectRequestRecord.ExtraInfo,
                GroupName = objectRequestRecord.GroupName,
                FirstName = user.As<UserDetailsPart>().FirstName,
                LastName = user.As<UserDetailsPart>().LastName
            };

            return View(viewModel);
        }
    }
}