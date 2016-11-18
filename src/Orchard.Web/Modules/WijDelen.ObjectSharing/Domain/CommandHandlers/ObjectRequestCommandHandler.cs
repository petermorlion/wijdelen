using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.CommandHandlers {
    public class ObjectRequestCommandHandler : ICommandHandler<RequestObject>, ICommandHandler<MarkSynonymAsOwned> {
        private readonly IEventSourcedRepository<ObjectRequest> _repository;

        public ObjectRequestCommandHandler(IEventSourcedRepository<ObjectRequest> repository) {
            _repository = repository;
        }
        public void Handle(RequestObject command) {
            var objectRequest = new ObjectRequest(command.ObjectRequestId, command.Description, command.ExtraInfo, command.UserId);
            _repository.Save(objectRequest, command.Id.ToString());
        }

        public void Handle(MarkSynonymAsOwned command) {
            // TODO: either continue this flow or implement differently
        }
    }
}