using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Localization;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.Fakes;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ArchetypesControllerTests {
        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT()
        {
            var controller = new ArchetypesController(null);
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void IndexShouldShowArchetypes() {
            var records = new[] {
                new ItemArchetypeRecord(),
                new ItemArchetypeRecord()
            };

            var repositoryMock = new Mock<IRepository<ItemArchetypeRecord>>();
            repositoryMock.SetRecords(records);

            var controller = new ArchetypesController(repositoryMock.Object);

            var result = controller.Index();

            ((ViewResult)result).Model.ShouldBeEquivalentTo(records);
        }
    }
}