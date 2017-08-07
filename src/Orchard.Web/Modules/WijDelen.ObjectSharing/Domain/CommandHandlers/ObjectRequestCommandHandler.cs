using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.CommandHandlers {
    public class ObjectRequestCommandHandler : 
        ICommandHandler<RequestObject>, 
        ICommandHandler<ConfirmObjectRequest>,
        ICommandHandler<DenyObjectRequest>,
        ICommandHandler<DenyObjectRequestForNow>,
        ICommandHandler<UnblockObjectRequests>,
        ICommandHandler<BlockObjectRequestByAdmin>,
        ICommandHandler<StopObjectRequest> {
        private readonly IEventSourcedRepository<ObjectRequest> _repository;

        public ObjectRequestCommandHandler(IEventSourcedRepository<ObjectRequest> repository) {
            _repository = repository;
        }
        public void Handle(RequestObject command) {
            var objectRequest = new ObjectRequest(command.ObjectRequestId, command.Description, command.ExtraInfo, command.UserId);
            _repository.Save(objectRequest, command.Id.ToString());
        }

        public void Handle(ConfirmObjectRequest command) {
            var objectRequest = _repository.Find(command.ObjectRequestId);
            objectRequest.Confirm(command.ConfirmingUserId);
            _repository.Save(objectRequest, command.Id.ToString());
        }
        public void Handle(DenyObjectRequest command) {
            var objectRequest = _repository.Find(command.ObjectRequestId);
            objectRequest.Deny(command.DenyingUserId);
            _repository.Save(objectRequest, command.Id.ToString());
        }

        public void Handle(DenyObjectRequestForNow command) {
            var objectRequest = _repository.Find(command.ObjectRequestId);
            objectRequest.DenyForNow(command.DenyingUserId);
            _repository.Save(objectRequest, command.Id.ToString());
        }

        public void Handle(UnblockObjectRequests command) {
            foreach (var objectRequestId in command.ObjectRequestIds) {
                var objectRequest = _repository.Find(objectRequestId);
                objectRequest.Unblock();
                _repository.Save(objectRequest, command.Id.ToString());
            }
        }

        public void Handle(BlockObjectRequestByAdmin command) {
            var objectRequest = _repository.Find(command.ObjectRequestId);
            objectRequest.Block(command.Reason);
            _repository.Save(objectRequest, command.Id.ToString());
        }

        public void Handle(StopObjectRequest command) {
            var objectRequest = _repository.Find(command.ObjectRequestId);
            objectRequest.Stop();
            _repository.Save(objectRequest, command.Id.ToString());
        }
    }
}