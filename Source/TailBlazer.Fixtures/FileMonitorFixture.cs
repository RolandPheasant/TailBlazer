
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
        public void Monitor_NoFilter()
        {
        
            var scrollRequest = new BehaviorSubject<ScrollRequest>(new ScrollRequest(10)); 
            var scheduler = new TestScheduler();

            using (var file = new TestFile("FileMonitorFixture.FileMonitor"))
            {
                file.Append(CreateLines(1, 100));

                var segments = file.Info.WatchFile(scheduler: scheduler).SegmentWithReport();
                using (var monitor = new FileMonitor(segments, scrollRequest, scheduler: scheduler))
                {
                    //monitors inital tail
                    scheduler.AdvanceByMilliSeconds(350);
                    var actual = TransformToString(monitor.Lines);
                    var expected = CreateLines(91, 10);
                    CollectionAssert.AreEqual(actual, expected);

                    //auto tails when new lines are added
                    file.Append(CreateLines(101, 5));
                    scheduler.AdvanceByMilliSeconds(250);
                    actual = TransformToString(monitor.Lines);
                    expected = CreateLines(96, 10);
                    CollectionAssert.AreEqual(actual, expected);

                    //scrolls to a specified position
                    scrollRequest.OnNext(new ScrollRequest(10, 80));
                    scheduler.AdvanceByMilliSeconds(250);
                    actual = TransformToString(monitor.Lines);
                    expected = CreateLines(80, 10);
                    CollectionAssert.AreEqual(actual, expected);

                    //scroll again
                    scrollRequest.OnNext(new ScrollRequest(10, 85));
                    scheduler.AdvanceByMilliSeconds(250);
                    actual = TransformToString(monitor.Lines);
                    expected = CreateLines(85, 10);
                    CollectionAssert.AreEqual(actual, expected);

                    //simulate a file roll-over
                    scrollRequest.OnNext(new ScrollRequest(10));
                    scheduler.AdvanceByMilliSeconds(250);
                    file.Delete();
                    file.Append(CreateLines(50, 5));
                    scheduler.AdvanceByMilliSeconds(2500);

                    actual = TransformToString(monitor.Lines);
                    expected = CreateLines(50, 5);
                    CollectionAssert.AreEqual(actual, expected);
                }
            }
        }

            
        [FactAttribute]
        public void Monitor_Filter()
        {

            var scrollRequest = new BehaviorSubject<ScrollRequest>(new ScrollRequest(10));
            var scheduler = new TestScheduler();

            using (var file = new TestFile("FileMonitorFixture.Filter"))
            {
                file.Append(1, 100);

                var segments = file.Info.WatchFile(scheduler: scheduler)
                    .SegmentWithReport();

                using (var monitor = new FileMonitor(segments, scrollRequest, str => str.Contains("9"), scheduler: scheduler))
                {
                    //monitors initial tail
                    scheduler.AdvanceByMilliSeconds(250);
                    var actual = TransformToString(monitor.Lines);
                    var expected = CreateLines(90, 10);
                    CollectionAssert.AreEqual(actual, expected);
                    

                    //When new lines not matching the filter are added, nothing should changed
                    file.Append(CreateLines(101, 5));
                    scheduler.AdvanceByMilliSeconds(250);
                    actual = TransformToString(monitor.Lines);
                    CollectionAssert.AreEqual(actual, expected);
                    
                    //add to see matching lines to see whether they are auto scrolled
                    var linesToAdd = CreateLines(new[] {9, 19, 29, 39, 49});
                    file.Append(linesToAdd);
                    scheduler.AdvanceByMilliSeconds(250);
                    actual = TransformToString(monitor.Lines);
                    expected = CreateLines(new[] { 95, 96, 97, 98, 99, 9, 19, 29, 39, 49 });
                    CollectionAssert.AreEqual(actual, expected);

                    //scroll to a specific position

                   // file.Append(CreateLines(98, 5));
                    scrollRequest.OnNext(new ScrollRequest(5, 10));
                    actual = TransformToString(monitor.Lines);
                    expected = CreateLines(new[] { 91, 92, 93, 94, 95});
                    CollectionAssert.AreEqual(actual, expected);

                    ////simulate a file roll-over
                    scrollRequest.OnNext(new ScrollRequest(5));
                    scheduler.AdvanceByMilliSeconds(250);
                    file.Delete();
                    file.Append(CreateLines(50, 100));
                    scheduler.AdvanceByMilliSeconds(2500);

                    actual = TransformToString(monitor.Lines);
                    expected = CreateLines(new[] { 109, 119, 129, 139, 149 }); ;
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
        private string[] CreateLines(int[] lines)
        {
            return lines.Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
        }
    }
}
