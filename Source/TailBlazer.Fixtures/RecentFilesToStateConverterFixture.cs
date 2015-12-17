using System.IO;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class RecentFilesToStateConverterFixture
    {
        [Fact]
        public void ConvertFiles()
        {

            var files = new[]
            {
                new RecentFile(new FileInfo(@"C:\\File1.txt")),
                new RecentFile(new FileInfo(@"C:\\File2.txt")),
            };

            var converter = new RecentFilesToStateConverter();

            var converted = converter.Convert(files);

        }
    }
}