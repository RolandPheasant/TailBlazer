using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using TailBlazer.Views.Formatting;

namespace TailBlazer.Fixtures
{
    public class DefaultIconSelectorFixture
    {
        [Theory,
            InlineData("DEBUG", false),
            InlineData("DEBUG", true),
            InlineData(null, true),
            InlineData(null, false),
            ]
        public void GetIconForShouldWork(string text, bool useRegex)
        {
            var selector = new DefaultIconSelector();

            var result = selector.GetIconFor(text, useRegex);

            result.Should().NotBeNullOrEmpty();
        }

        [Theory,
            InlineData("DEBUG", true, "INFO"),
            InlineData("DEBUG", false, "INFO"),
            InlineData("DEBUG", true, "xxxxxxx"),
            InlineData("DEBUG", false, "xxxxxxx"),
            InlineData("Bug", false, "xxxxxxx"),
            InlineData("DEBUG", false, "xxxxxxx"),
            ]
        public void GetIconOrDefaultShouldWork(string text, bool useRegex, string iconKind)
        {
            var selector = new DefaultIconSelector();

            var result = selector.GetIconOrDefault(text, useRegex, iconKind);

            result.Should().NotBeNullOrEmpty();
        }
    }
}
