using System.Linq;
using FluentAssertions;
using TailBlazer.Domain.Formatting;
using TailBlazer.Infrastucture.Virtualisation;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class DisplayTextFixture
    {
        [Fact]
        public void CanVirtualise()
        {
            var input = new[]
            {
                new MatchedString("The cat "),
                new MatchedString("sat "),
                new MatchedString("on the mat"),
                 new MatchedString(" and slept like a bat")
            }.Select(ms => new DisplayText(ms))
            .ToArray();


            var expected = new[]
            {
                new MatchedString("at "),
                new MatchedString("sat "),
                new MatchedString("on the m")
            }
            .Select(ms => new DisplayText(ms))
            .ToArray();

            var result = input.Virtualise(new TextScrollInfo(5, 15)).ToArray();


            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void CanVirtualise2()
        {
            var input = new[]
            {
                new MatchedString("The cat "),
                new MatchedString("sat "),
                new MatchedString("on the mat"),
                 new MatchedString(" and slept like a bat")
            }.Select(ms => new DisplayText(ms))
            .ToArray();


            var expected = new[]
            {
                new MatchedString("t "),
                new MatchedString("on the m")
            }
            .Select(ms => new DisplayText(ms))
            .ToArray();

            var result = input.Virtualise(new TextScrollInfo(10, 10)).ToArray();


            result.Should().BeEquivalentTo(expected);
        }
    }
}