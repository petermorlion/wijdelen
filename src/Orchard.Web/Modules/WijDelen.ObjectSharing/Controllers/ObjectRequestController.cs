using System;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Themes;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Controllers {
    [Themed]
    public class ObjectRequestController : Controller {
        private readonly ICommandHandler<RequestObject> _requestObjectCommandHandler;

        public ObjectRequestController(ICommandHandler<RequestObject> requestObjectCommandHandler)
        {
            _requestObjectCommandHandler = requestObjectCommandHandler;
            T = NullLocalizer.Instance;
        }

        public ActionResult New() {
            return View(new NewObjectRequestViewModel());
        }

        [HttpPost]
        public ActionResult New(NewObjectRequestViewModel viewModel) {
            if (string.IsNullOrWhiteSpace(viewModel.Description)) {
                ModelState.AddModelError("Description", T("Please provide a description of the item you need."));
            }

            if (string.IsNullOrWhiteSpace(viewModel.ExtraInfo))
            {
                ModelState.AddModelError("ExtraInfo", T("Please provide some extra info."));
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var command = new RequestObject(viewModel.Description, viewModel.ExtraInfo);
            _requestObjectCommandHandler.Handle(command);

            return View(viewModel);
        }

        //public ActionResult Index(Guid id) {

        //}

        public Localizer T { get; set; }
    }
}