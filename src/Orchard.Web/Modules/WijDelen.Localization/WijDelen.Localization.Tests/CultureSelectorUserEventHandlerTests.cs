using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Localization.Providers;

namespace WijDelen.Localization.Tests {
    [TestFixture]
    public class CultureSelectorUserEventHandlerTests {
        private Mock<ICultureStorageProvider> _cultureStorageProviderMock;
        private CultureSelectorUserEventHandler _userEventHandler;

        [SetUp]
        public void Init()
        {
            _cultureStorageProviderMock = new Mock<ICultureStorageProvider>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<CultureSelectorUserEventHandler>();
            containerBuilder.RegisterInstance(_cultureStorageProviderMock.Object).As<ICultureStorageProvider>();

            var container = containerBuilder.Build();

            _userEventHandler = container.Resolve<CultureSelectorUserEventHandler>();
        }

        [Test]
        public void WhenUserLoggedIn_ShouldSetCulture() {
            var userMockFactory = new UserMockFactory();
            var user = userMockFactory.Create("john.doe@example.com", "john.doe@example.com", "John", "Doe", "en-US");

            _userEventHandler.LoggedIn(user);

            _cultureStorageProviderMock.Verify(x => x.SetCulture("en-US"));
        }
    }
}