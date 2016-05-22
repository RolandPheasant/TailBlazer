using System.Linq;
using FluentAssertions;
using TailBlazer.Views.Formatting;
using Xunit;

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
