using System;
using System.IO;
using System.Linq;
using System.Text;
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
            using (var file=new TestFile())
            {
                file.Append(Enumerable.Range(1, 100).Select(i => i.ToString()));
                var lines = file.Info.ReadLines(new[] { 1, 2, 3, 10, 100, 105 });
                lines.Select(l => l.Number).ShouldAllBeEquivalentTo(new[] { 1, 2, 3, 10, 100 });
            }


        }
    }
}