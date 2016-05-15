using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using TailBlazer.Views.Formatting;
using MaterialDesignThemes.Wpf;

namespace TailBlazer.Fixtures
{
    public class IconDescriptionFixture
    {
        [Fact]
        public void IconDescriptionShoudBeComparable()
        {
            var id1 = new IconDescription(new PackIconKind(), "test");
            var id2 = new IconDescription(new PackIconKind(), "test");
 
            var result = id1 == id2;

            result.Should().BeTrue();
        }

        [Fact]
        public void IconDescriptionShoudBeComparableInEqual()
        {
            var id1 = new IconDescription(new PackIconKind(), "test1");
            var id2 = new IconDescription(new PackIconKind(), "test2");

            var result = id1 == id2;

            result.Should().BeFalse();
        }

    }
}
