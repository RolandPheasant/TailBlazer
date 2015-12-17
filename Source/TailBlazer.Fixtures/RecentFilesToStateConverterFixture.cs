using System.IO;
using FluentAssertions;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class RecentFilesToStateConverterFixture
    {
        [Fact]
        public void TwoWayConversion()
        {

            var files = new[]
            {
                new RecentFile(new FileInfo(@"C:\\File1.txt")),
                new RecentFile(new FileInfo(@"C:\\File2.txt")),
            };

            var converter = new RecentFilesToStateConverter();
            var state = converter.Convert(files);
            var restored = converter.Convert(state);
            restored.ShouldAllBeEquivalentTo(files);
        }
    }
}