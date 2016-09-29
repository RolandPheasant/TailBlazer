using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class FileTailMonitorFixture
    {
        [Fact]
        public void MonitorTail()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Append(CreateLines(1, 100));

                var autoTailer = new FileTailReader(file.Info.WatchFile(scheduler: scheduler).WithSegments());

                FileTailInfo result = null;
                int counter = 0;
                using (autoTailer.Tail().Subscribe(x => { result = x; counter++; }))
                {
                    //check that intial values are loaded
                    scheduler.AdvanceBySeconds(250);
                    counter.Should().Be(1);
                    result.Count.Should().Be(100);
                    var expected = CreateLines(1, 100);
                    result.Tail.Select(l => l.Text).ShouldBeEquivalentTo(expected);

                    //check subsequent tail is received
                    file.Append(CreateLines(101, 5));
                    scheduler.AdvanceBySeconds(1);
                    counter.Should().Be(2);
                    result.Count.Should().Be(5);
                    expected = CreateLines(101, 5);
                    result.Tail.Select(l => l.Text).ShouldBeEquivalentTo(expected);
                }
            }
        }

        private string[] CreateLines(int start, int take)
        {
            return Enumerable.Range(start, take).Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
        }
    }
}