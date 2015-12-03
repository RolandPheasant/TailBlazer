using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class FileSearchFixture
    {
        [Fact]
        public void SearchWillReturnSomething()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            var scheduler = new TestScheduler();
            var pulse = new Subject<Unit>();
            
            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());

            var segments = info
                        .WatchFile(pulse)
                        .WithSegments();

            using (var search = new FileSearch(segments, str => str.Contains("9")))
            {

            }

            File.Delete(file);
        }
    }
}