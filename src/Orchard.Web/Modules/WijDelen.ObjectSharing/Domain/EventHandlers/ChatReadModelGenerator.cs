using Orchard.ContentManagement;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.UserImport.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ChatReadModelGenerator : IEventHandler<ChatStarted> {
        private readonly IRepository<ChatRecord> _repository;
        private readonly IGetUserByIdQuery _userQuery;

        public ChatReadModelGenerator(IRepository<ChatRecord> repository, IGetUserByIdQuery userQuery) {
            _repository = repository;
            _userQuery = userQuery;
        }

        public void Handle(ChatStarted e) {
            var requestingUser = _userQuery.GetResult(e.RequestingUserId);
            var confirmingUser = _userQuery.GetResult(e.ConfirmingUserId);
        
            var newRecord = new ChatRecord {
                ChatId = e.SourceId,
                ObjectRequestId = e.ObjectRequestId,
                RequestingUserId = e.RequestingUserId,
                RequestingUserName = $"{requestingUser.As<UserDetailsPart>().FirstName} {requestingUser.As<UserDetailsPart>().LastName}",
                ConfirmingUserId = e.ConfirmingUserId,
                ConfirmingUserName = $"{confirmingUser.As<UserDetailsPart>().FirstName} {confirmingUser.As<UserDetailsPart>().LastName}"
            };

            _repository.Create(newRecord);
        }
    }
}