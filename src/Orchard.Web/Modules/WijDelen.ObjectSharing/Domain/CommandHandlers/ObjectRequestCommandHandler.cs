using System;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.CommandHandlers {
    public class ObjectRequestCommandHandler : ICommandHandler<RequestObject> {
        private readonly IEventSourcedRepository<ObjectRequest> _repository;

        public ObjectRequestCommandHandler(IEventSourcedRepository<ObjectRequest> repository) {
            _repository = repository;
        }
        public void Handle(RequestObject command) {
            var objectRequest = new ObjectRequest(Guid.NewGuid(), command.Description, command.ExtraInfo);
            _repository.Save(objectRequest, command.Id.ToString());
        }
    }
}