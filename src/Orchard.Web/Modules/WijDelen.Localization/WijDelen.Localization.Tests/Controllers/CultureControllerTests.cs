using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Localization.Providers;
using WijDelen.Localization.Controllers;

namespace WijDelen.Localization.Tests.Controllers {
    [TestFixture]
    public class CultureControllerTests {
        private CultureController _controller;
        private Mock<ICultureStorageProvider> _cultureStorageProviderMock;

        [SetUp]
        public void Init() {
            _cultureStorageProviderMock = new Mock<ICultureStorageProvider>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<CultureController>();
            containerBuilder.RegisterInstance(_cultureStorageProviderMock.Object).As<ICultureStorageProvider>();

            var container = containerBuilder.Build();

            _controller = container.Resolve<CultureController>();
        }

        [Test]
        public void TestChangeCulture() {
            var result = _controller.ChangeCulture("en-US", "foo");

            _cultureStorageProviderMock.Verify(x => x.SetCulture("en-US"));

            result.Should().BeOfType<RedirectResult>();
        }
    }
}