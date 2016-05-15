using System.Linq;
using Xunit;
using FluentAssertions;
using TailBlazer.Views.Formatting;

namespace TailBlazer.Fixtures
{
    public class IconProviderFixture
    {
        [Fact]
        public void IconProviderShouldHaveIcons()
        {
            using (var provider = new IconProvider(new DefaultIconSelector()))
            {
                var result = provider.Icons;

                result.Items.Any().Should().BeTrue();
            }
        }
    }
}
