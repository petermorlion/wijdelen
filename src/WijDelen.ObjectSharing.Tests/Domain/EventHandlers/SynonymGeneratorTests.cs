using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class SynonymGeneratorTests {
        [Test]
        public void ShouldCreateNewSynonymIfNotExists() {
            var objectRequested = new ObjectRequested { Description = "Sneaky sneakers" };

            var synonymFactory = new SynonymFactory();
            var synonym = synonymFactory.Create("");

            var contentManagerMock = new Mock<IContentManager>();
            contentManagerMock.Setup(x => x.New("Synonym")).Returns(synonym);

            var synonymQuery = new Mock<IFindSynonymsByExactMatchQuery>();
            synonymQuery.Setup(x => x.GetResults("Sneaky sneakers")).Returns(new ContentItem[0]);

            var generator = new SynonymGenerator(contentManagerMock.Object, synonymQuery.Object);

            generator.Handle(objectRequested);

            synonym.As<TitlePart>().Title.Should().Be("Sneaky sneakers");
            contentManagerMock.Verify(x => x.Create(synonym));
            contentManagerMock.Verify(x => x.Publish(synonym));
        }

        [Test]
        public void ShouldNotCreateNewSynonymIfExists()
        {
            var objectRequested = new ObjectRequested { Description = "Sneaky sneakers" };

            var synonymFactory = new SynonymFactory();
            var synonym = synonymFactory.Create("");
            var existingSynonym = synonymFactory.Create("Sneaky sneakers");

            var contentManagerMock = new Mock<IContentManager>();
            contentManagerMock.Setup(x => x.New("Synonym")).Returns(synonym);

            var synonymQuery = new Mock<IFindSynonymsByExactMatchQuery>();
            synonymQuery.Setup(x => x.GetResults("Sneaky sneakers")).Returns(new [] { existingSynonym });

            var generator = new SynonymGenerator(contentManagerMock.Object, synonymQuery.Object);

            generator.Handle(objectRequested);

            contentManagerMock.Verify(x => x.Create(synonym), Times.Never);
            contentManagerMock.Verify(x => x.Publish(synonym), Times.Never);
        }
    }
}