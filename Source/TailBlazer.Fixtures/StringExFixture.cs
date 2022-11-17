using System;
using System.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace TailBlazer.Fixtures
{
    public class StringExFixture
    {
        private readonly ITestOutputHelper output;

        public StringExFixture(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void FormatWithAbbreviation()
        {
            long x = 1024 * 1024;
            var result = x.FormatWithAbbreviation();

            result.Should().BeEquivalentTo("1 MB");
        }

        [Fact]
        public void FormatWithAbbreviationShouldHandleMinValue()
        {
            var result = long.MinValue.FormatWithAbbreviation();

            result.Should().BeEquivalentTo("-9223372036854775808 B");
        }

        [Fact]
        public void FormatWithAbbreviationShouldHandleMaxValue()
        {
            var result = long.MaxValue.FormatWithAbbreviation();

            result.Should().BeEquivalentTo("8 EB");
        }

        [Theory
            , InlineData((long)12 * 1024 * 1024)
            , InlineData((long)12 * 1024 * 1024 + 1234567)
            , InlineData((long)3 * 1024 * 1024 * 1024 + 987652342)]
        public void FormatWithAbbreviationShouldBeEquivalentToFormatWithAbbreviationOld(long input)
        {
            var watch = new Stopwatch();
            watch.Start();
            var result = input.FormatWithAbbreviation();
            watch.Stop();
            var resultTime = watch.Elapsed;

            watch.Start();
            var resultOld = input.FormatWithAbbreviationOld();
            watch.Stop();

            var resultOldTime = watch.Elapsed;

            result.Should().Be(resultOld);
            resultTime.Should().BeLessThan(resultOldTime);

            output.WriteLine($"calculation for {result} took new: {resultTime} old: {resultOldTime}");
        }
    }

    public static class OldMethods
    {
        public static string FormatWithAbbreviationOld(this long source)
        {
            //TODO: not very efficient. Come back to this later
            //var powereof = Math.Floor(Math.Log10(source));
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = source;
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
