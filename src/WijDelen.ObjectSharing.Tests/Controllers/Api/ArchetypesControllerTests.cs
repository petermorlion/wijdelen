using System.Collections.Generic;
using System.Web.Http.Results;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Controllers.Api;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.Fakes;

namespace WijDelen.ObjectSharing.Tests.Controllers.Api {
    [TestFixture]
    public class ArchetypesControllerTests {
        [Test]
        public void GetShouldReturnExactMatch()
        {
            var records = new[] {
                new ArchetypeRecord { Name = "Sneakers" },
                new ArchetypeRecord { Name = "Flaming Moe" }
            };

            var repositoryMock = new Mock<IRepository<ArchetypeRecord>>();
            repositoryMock.SetRecords(records);

            var controller = new ArchetypesController(repositoryMock.Object, null);

            var result = (OkNegotiatedContentResult<List<string>>)controller.Get("sneakers");

            result.Content.ShouldBeEquivalentTo(new List<string> { "Sneakers" });
        }
    }
}