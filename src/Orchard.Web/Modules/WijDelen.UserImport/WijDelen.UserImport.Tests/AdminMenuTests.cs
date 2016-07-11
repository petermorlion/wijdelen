using System.Linq;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace WijDelen.UserImport.Tests {
    [TestFixture]
    public class AdminMenuTests {
        [Test]
        public void TestMenuName() {
            Assert.AreEqual("admin", new AdminMenu().MenuName);
        }

        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT()
        {
            var menu = new AdminMenu();
            var localizer = NullLocalizer.Instance;

            menu.T = localizer;

            Assert.AreEqual(localizer, menu.T);
        }

        [Test]
        public void TestGetNavigation() {
            var menu = new AdminMenu();
            var navigationBuilder = new NavigationBuilder();

            menu.GetNavigation(navigationBuilder);
            var navigation = navigationBuilder.Build();

            Assert.AreEqual(1, navigation.Count());
            Assert.AreEqual("User Import", navigation.Single().Text.ToString());
            Assert.AreEqual("12", navigation.Single().Position);
            Assert.AreEqual("WijDelen.UserImport", navigation.Single().RouteValues["area"]);
        }
    }
}