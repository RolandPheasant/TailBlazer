
using System.Linq;
using System.Reactive;
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
            var scheduler = new TestScheduler();
            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            {
                file.Append(Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());

                var segments = file.Info.WatchFile(pulse).WithSegments();

                using (var search = new FileSearch(segments, str => str.Contains("9")))
                {
                    pulse.Once();
                    file.Append(Enumerable.Range(101, 10).Select(i => i.ToString()).ToArray());
                    pulse.Once();
                }
            }
        }
    }
}