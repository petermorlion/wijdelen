using FluentAssertions;
using NUnit.Framework;

namespace Peergroups.Theme.Tests
{
    [TestFixture]
    public class MobileShapeTableProviderTests
    {
        [Test]
        public void IsMobileBrowser_ForAndroidPhone_ShouldReturnTrue() {
            var userAgent = "Mozilla/5.0 (Linux; Android 4.0.4; Galaxy Nexus Build/IMM76B) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.133 Mobile Safari/535.19";

            MobileShapeTableProvider.IsMobileBrowser(userAgent).Should().BeTrue();
        }
    }
}
