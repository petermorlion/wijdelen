using System.Web.Mvc;
using Orchard.Data;
using Orchard.UI.Admin;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ObjectRequestDetailsAdminController : Controller
    {
        private readonly IRepository<ObjectRequestRecord> _objectRequestRecordRepository;

        public ObjectRequestDetailsAdminController(IRepository<ObjectRequestRecord> repository) {
            _objectRequestRecordRepository = repository;
        }

        public ViewResult Index(int objectRequestId) {
            return View();
        }
    }
}