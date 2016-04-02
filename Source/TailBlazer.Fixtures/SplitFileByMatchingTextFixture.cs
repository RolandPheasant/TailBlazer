using System.Collections;
using System.Linq;
using FluentAssertions;
using TailBlazer.Domain.Formatting;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class SplitFileByMatchingTextFixture
    {

        [Fact]
        public void FindMatchingText()
        {
      
            var input = "The lazy cat could not catch a mouse";

            var matched = input.MatchString("cat").ToArray();
            var joined = matched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);

            var multimatched = input.MatchString(new [] { "cat", "lazy" }).ToArray();
            var multijoined = multimatched.Select(m => m.Part).ToDelimited("");
            multijoined.Should().Be(input);
        }

        [Fact]
        public void FindWithNoMatch()
        {
            var input = "The lazy cat could not catch a mouse";

            var matched = input.MatchString("energetic").ToArray();
            var joined = matched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);

            var multimatched = input.MatchString(new[] { "dog", "energetic" }).ToArray();
            var multijoined = multimatched.Select(m => m.Part).ToDelimited("");
            multijoined.Should().Be(input);
        }

        [Fact]
        public void MatchAtEnd()
        {
            var input = "The lazy cat could not catch a mouse";

            var matched = input.MatchString("mouse").ToArray();
            var joined = matched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);


            var multimatched = input.MatchString(new[] { "mouse" }).ToArray();
            var multijoined = multimatched.Select(m => m.Part).ToDelimited("");
            multijoined.Should().Be(input);
        }

        [Fact]
        public void MatchAtStart()
        {
            var input = "The lazy cat could not catch a mouse";

            var matched = input.MatchString("The").ToArray();
            var joined = matched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);

            var multimatched = input.MatchString(new[] { "The","not" }).ToArray();
            var multijoined = multimatched.Select(m => m.Part).ToDelimited("");
            multijoined.Should().Be(input);
        }

        [Fact]
        public void NoMatch()
        {
            var input = "The lazy cat could not catch a mouse";
            var matched = input.MatchString("XXX").ToArray();
            var joined = matched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);

            var multimatched = input.MatchString(new[] { "XXX", "XXX" }).ToArray();
            var multijoined = multimatched.Select(m => m.Part).ToDelimited("");
            multijoined.Should().Be(input);
        }

    }
}
