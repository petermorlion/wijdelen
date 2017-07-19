using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;
using WijDelen.UserImport.Services;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    /// <summary>
    /// Generates the read model of an object request.
    /// </summary>
    public class ObjectRequestReadModelGenerator : IEventHandler<ObjectRequested>, IEventHandler<ObjectRequestUnblocked> {
        private readonly IRepository<ObjectRequestRecord> _repository;
        private readonly IGroupService _groupService;

        public ObjectRequestReadModelGenerator(IRepository<ObjectRequestRecord> repository, IGroupService groupService) {
            _repository = repository;
            _groupService = groupService;
        }

        public void Handle(ObjectRequested e) {
            var existingRecord = _repository.Get(x => x.AggregateId == e.SourceId);
            if (existingRecord != null) {
                return;
            }

            var group = _groupService.GetGroupForUser(e.UserId);

            var newRecord = new ObjectRequestRecord {
                AggregateId = e.SourceId,
                Description = e.Description,
                ExtraInfo = e.ExtraInfo,
                Version = e.Version,
                UserId = e.UserId,
                CreatedDateTime = e.CreatedDateTime,
                GroupId = @group.Id,
                GroupName = @group.Name,
                Status = e.Status.ToString()
            };

            _repository.Create(newRecord);
        }

        public void Handle(ObjectRequestUnblocked e) {
            var existingRecord = _repository.Get(x => x.AggregateId == e.SourceId);
            if (existingRecord == null)
            {
                return;
            }

            existingRecord.Status = e.Status.ToString();
            existingRecord.Version = e.Version;
            _repository.Update(existingRecord);
        }
    }
}