using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Infrastructure;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Controllers {
    [Admin]
    public class FeedAdminController : Controller {
        private readonly IRepository<EventRecord> _eventRepository;
        private readonly IRepository<FeedItemRecord> _feedItemRepository;
        private readonly IFeedReadModelGenerator _feedReadModelGenerator;
        private readonly IOrchardServices _orchardServices;

        public FeedAdminController(
            IFeedReadModelGenerator feedReadModelGenerator,
            IOrchardServices orchardServices,
            IRepository<EventRecord> eventRepository,
            IRepository<FeedItemRecord> feedItemRepository) {
            _feedReadModelGenerator = feedReadModelGenerator;
            _orchardServices = orchardServices;
            _eventRepository = eventRepository;
            _feedItemRepository = feedItemRepository;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageFeeds, T("You are not authorized to manage feeds."))) return new HttpUnauthorizedResult();

            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageFeeds, T("You are not authorized to manage feeds."))) return new HttpUnauthorizedResult();

            foreach (var feedItem in _feedItemRepository.Table.ToList()) {
                _feedItemRepository.Delete(feedItem);
            }

            var events = _eventRepository.Fetch(x => true).OrderBy(x => x.Timestamp).ToList();
            var jsonSerializerSettings = new VersionedEventSerializerSettings();

            var counter = 0;

            foreach (var eventRecord in events) {
                var versionedEvent = JsonConvert.DeserializeObject(eventRecord.Payload, jsonSerializerSettings);
                if (versionedEvent.GetType() == typeof(ObjectRequested)) {
                    _feedReadModelGenerator.Handle((ObjectRequested) versionedEvent);
                    counter += 1;
                }
                else if (versionedEvent.GetType() == typeof(ObjectRequestConfirmed)) {
                    _feedReadModelGenerator.Handle((ObjectRequestConfirmed) versionedEvent);
                    counter += 1;
                }
                else if (versionedEvent.GetType() == typeof(ChatMessageAdded)) {
                    _feedReadModelGenerator.Handle((ChatMessageAdded) versionedEvent);
                    counter += 1;
                }
                else if (versionedEvent.GetType() == typeof(ChatStarted)) {
                    _feedReadModelGenerator.Handle((ChatStarted) versionedEvent);
                    counter += 1;
                }
                else if (versionedEvent.GetType() == typeof(ObjectRequestDenied)) {
                    _feedReadModelGenerator.Handle((ObjectRequestDenied) versionedEvent);
                    counter += 1;
                }
                else if (versionedEvent.GetType() == typeof(ObjectRequestDeniedForNow)) {
                    _feedReadModelGenerator.Handle((ObjectRequestDeniedForNow) versionedEvent);
                    counter += 1;
                }
            }

            _orchardServices.Notifier.Add(NotifyType.Success, T("{0} events processed succesfully.", counter));

            return RedirectToAction("Index");
        }
    }
}