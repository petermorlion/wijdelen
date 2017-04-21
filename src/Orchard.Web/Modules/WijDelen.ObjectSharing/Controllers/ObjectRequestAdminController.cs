using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.UI.Admin;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class ObjectRequestAdminController : Controller {
        private readonly IRepository<ObjectRequestRecord> _objectRequestRecordRepository;

        public ObjectRequestAdminController(IRepository<ObjectRequestRecord> objectRequestRecordRepository) {
            _objectRequestRecordRepository = objectRequestRecordRepository;
        }

        public ActionResult Index(int skip = 0) {
            var records = _objectRequestRecordRepository.Table
                .OrderByDescending(x => x.CreatedDateTime)
                .Skip(skip)
                .Take(50)
                .ToList();

            return View(records);
        }
    }
}