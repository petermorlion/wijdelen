using System;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ObjectRequestMailer : IEventHandler<ObjectRequested>
    {
        private readonly IEventSourcedRepository<ObjectRequestMail> _repository;

        public ObjectRequestMailer(IEventSourcedRepository<ObjectRequestMail> repository) {
            _repository = repository;
        }

        public void Handle(ObjectRequested objectRequested) {
            var objectRequestMail = new ObjectRequestMail(
                Guid.NewGuid(), 
                objectRequested.UserId, 
                new string[0], 
                objectRequested.Description, 
                objectRequested.ExtraInfo);

            _repository.Save(objectRequestMail, Guid.NewGuid().ToString());
        }
    }
}