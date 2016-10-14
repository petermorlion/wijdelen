using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Security;
using Orchard.UI.Notify;

namespace WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes {
    public class FakeOrchardServices : IOrchardServices {
        public IContentManager ContentManager { get; }
        public ITransactionManager TransactionManager { get; }
        public IAuthorizer Authorizer { get; }
        public INotifier Notifier { get; }
        public dynamic New { get; }
        public WorkContext WorkContext { get; } = new FakeWorkContext();
    }
}