
using System.Linq;
using System.Reactive.Subjects;
using DynamicData;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class FileMonitorFixture
    {
        [Fact]
        public void CanReadIndiciesBack_SmallFile()
        {
        
            var scrollRequest = new BehaviorSubject<ScrollRequest>(new ScrollRequest(10)); 
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Append(CreateLines(1, 100));

                var segments = file.Info.WatchFile(scheduler: scheduler).WithSegments().WithTail();
                using (var monitor = new FileMonitor(segments, scrollRequest, scheduler: scheduler))
                {
                    //monitors inital tail
                    scheduler.AdvanceByMilliSeconds(250);
                    var actual = TransformToString(monitor.Lines);
                    var expected = CreateLines(91, 10);
                    CollectionAssert.AreEqual(actual, expected);

                    //autotails
                    file.Append(CreateLines(101, 5));
                    scheduler.AdvanceByMilliSeconds(250);
                    actual = TransformToString(monitor.Lines);
                    expected = CreateLines(96, 10);
                    CollectionAssert.AreEqual(actual, expected);

                    scrollRequest.OnNext(new ScrollRequest(10, 80));
                    scheduler.AdvanceByMilliSeconds(250);
                    actual = TransformToString(monitor.Lines);
                    expected = CreateLines(80, 10);
                    CollectionAssert.AreEqual(actual, expected);

                    scrollRequest.OnNext(new ScrollRequest(10, 85));
                    scheduler.AdvanceByMilliSeconds(250);
                    actual = TransformToString(monitor.Lines);
                    expected = CreateLines(85, 10);
                    CollectionAssert.AreEqual(actual, expected);
                }
            }

        }

        private string[] TransformToString(IObservableCache<Line, LineKey> cache)
        {
            return cache.Items.OrderBy(l=>l.LineInfo.Start).Select(l => l.Text).ToArray();
        }
        private string[] CreateLines(int start, int take)
        {
            return Enumerable.Range(start, take).Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
        }
    }
}
