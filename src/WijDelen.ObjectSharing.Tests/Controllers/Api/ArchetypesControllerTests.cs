using System.Collections.Generic;
using System.Web.Http.Results;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Controllers.Api;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;

namespace WijDelen.ObjectSharing.Tests.Controllers.Api
{
    [TestFixture]
    public class ArchetypesControllerTests
    {
        [Test]
        public void GetShouldReturnMatches()
        {
            var archetypeFactory = new ArchetypeFactory();
            var archetypes = new[] {
                archetypeFactory.Create("Sneakers"),
                archetypeFactory.Create("Sneaky snakes")
            };

            var queryMock = new Mock<IFindArchetypesByTitleQuery>();
            queryMock.Setup(x => x.GetResult("sneakers")).Returns(archetypes);

            var controller = new ArchetypesController(queryMock.Object);

            var result = (OkNegotiatedContentResult<List<string>>)controller.Get("sneakers");

            result.Content.ShouldBeEquivalentTo(new List<string> { "Sneakers", "Sneaky snakes" });
        }
    }
}