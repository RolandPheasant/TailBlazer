using System;
using System.Linq;
using FluentAssertions;
using TailBlazer.Domain.Formatting;
using TailBlazer.Views.Formatting;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class ColourProviderFixture
    {
        [Fact]
        public void ColourProviderLookupShouldFindAHue()
        {
            var provider = new ColourProvider();
            var key = new HueKey("amber", "Accent100");

            var result = provider.Lookup(key);

            result.HasValue.Should().Be(true);
            result.Value.Key.Should().Be(key);
        }

        [Fact]
        public void ColourProviderLookupWithIncorrectKeyShouldReturnEmptyValue()
        {
            var provider = new ColourProvider();
            var key = new HueKey("xxxxxxxx", "yyyyyyyyyy");

            var result = provider.Lookup(key);

            result.HasValue.Should().Be(false);
        }

        [Fact]
        public void ColourProviderLookupWithNullKeyShouldReturnEmptyValue()
        {
            var provider = new ColourProvider();
            HueKey key = default; 
            var result = provider.Lookup(key);

            result.HasValue.Should().Be(false);
        }

        [Fact]
        public void ColourProviderHuesShouldNotBeEmpty()
        {
            var provider = new ColourProvider();

            var result = provider.Hues;

            result.Any().Should().BeTrue();
        }

        [Fact]
        public void ColourProviderLookupShouldFindAllHues()
        {
            var provider = new ColourProvider();

            foreach (var hue in provider.Hues)
            {
                provider.Lookup(hue.Key).HasValue.Should().BeTrue();
            }
        }

        [Fact]
        public void ColourProviderGetAccentShouldSupportAllThemes()
        {
            var provider = new ColourProvider();

            foreach (var theme in Enum.GetValues(typeof(Theme)))
            {
                provider.GetAccent((Theme)theme).Should().NotBeNull();
            }
        }

        [Fact]
        public void ColourProviderDefaultAccentShouldReturnSomething()
        {
            var provider = new ColourProvider();

            provider.DefaultAccent.Should().NotBeNull();
        }

    }
}
