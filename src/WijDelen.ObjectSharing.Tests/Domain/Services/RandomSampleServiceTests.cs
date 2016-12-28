using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Services;

namespace WijDelen.ObjectSharing.Tests.Domain.Services {
    [TestFixture]
    public class RandomSampleServiceTests {
        [Test]
        public void WhenPassingInNull_ThrowException() {
            var service = new RandomSampleService();

            Exception thrownException = null;

            try {
                service.GetRandomSample<string>(null, 3);
            }
            catch (Exception e) {
                thrownException = e;
            }

            thrownException.Should().NotBeNull();
        }

        [Test]
        public void WhenPassingInSmallerCollection_ReturnCollection() {
            var service = new RandomSampleService();
            var list = new List<string> {
                "One",
                "Two"
            };

            var result = service.GetRandomSample(list, 20);

            result.ShouldBeEquivalentTo(list);
        }

        [Test]
        public void WhenPassingInLargerCollection_ReturnRandomSample() {
            var service = new RandomSampleService();
            var list = new List<string> {
                "One",
                "Two",
                "Three",
                "Four",
                "Five",
                "Six",
                "Seven",
                "Eight",
                "Nine",
                "Ten"
            };

            var result = service.GetRandomSample(list, 5);

            result.Count().Should().Be(5);

            foreach (var item in result) {
                list.Should().Contain(item);
                result.Count(x => x == item).Should().Be(1);
            }
        }
    }
}