using System.IO;
using System.Linq;
using FluentAssertions;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class ReadLinesFixture
    {
        [Fact]
        public void ReadSpecificFileLines()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => i.ToString()));

            var lines = info.ReadLines(new[] { 1, 2, 3, 10, 100, 105 });

            lines.Select(l=>l.Number).ShouldAllBeEquivalentTo(new[] { 1, 2, 3, 10, 100 });

            File.Delete(file);
        }
    }
}