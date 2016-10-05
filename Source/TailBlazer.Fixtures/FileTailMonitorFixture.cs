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

                var autoTailer = new FileTailReader(file.Info.WatchFile(scheduler: scheduler).WithSegments(7500));

                TailInfo result = null;
                int counter = 0;
                using (autoTailer.Tail().Subscribe(x => { result = x; counter++; }))
                {
                    //check that intial values are loaded
                    scheduler.AdvanceBySeconds(250);
                    counter.Should().Be(1);
                    result.Count.Should().Be(100);
                    var expected = CreateLines(1, 100);
                    result.Lines.Select(l => l.Text).ShouldBeEquivalentTo(expected);

                    //check subsequent tail is received
                    file.Append(CreateLines(101, 5));
                    scheduler.AdvanceBySeconds(1);
                    counter.Should().Be(2);
                    result.Count.Should().Be(5);
                    expected = CreateLines(101, 5);
                    result.Lines.Select(l => l.Text).ShouldBeEquivalentTo(expected);
                }
            }
        }

        [Fact]
        public void MonitorTailWithSegments()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Append(CreateLines(1, 100));

                var autoTailer = file.Info.WatchFile(scheduler: scheduler).WithSegments().WithTail();

                FileSegmentsWithTail result = null;
                int counter = 0;
                using (autoTailer.Subscribe(x => { result = x; counter++; }))
                {
                    //check that intial values are loaded
                    scheduler.AdvanceBySeconds(250);
                    counter.Should().Be(1);
                    result.Segments.Count.Should().Be(1);
                    var expected = CreateLines(1, 100);
                    result.TailInfo.Lines.Select(l => l.Text).ShouldBeEquivalentTo(expected);

                    //check subsequent tail is received
                    file.Append(CreateLines(101, 5));
                    scheduler.AdvanceBySeconds(1);
                    //3 in counter as an update will notify that there has been a segment and a tail change
                    counter.Should().Be(3);
                    result.TailInfo.Count.Should().Be(5);
                    result.Segments.Count.Should().Be(1);
                    expected = CreateLines(101, 5);
                    result.TailInfo.Lines.Select(l => l.Text).ShouldBeEquivalentTo(expected);
                }
            }
        }

        [Fact]
        public void ShouldNotNotifyNeedlessly()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Append(CreateLines(1, 100));

                var autoTailer = file.Info.WatchFile(scheduler: scheduler).WithSegments().WithTail();

                FileSegmentsWithTail result = null;
                int counter = 0;
                using (autoTailer.Subscribe(x => { result = x; counter++; }))
                {
                    //check that intial values are loaded
                    scheduler.AdvanceBySeconds(250);
                    counter.Should().Be(1);
                    result.Segments.Count.Should().Be(1);
                    var expected = CreateLines(1, 100);
                    result.TailInfo.Lines.Select(l => l.Text).ShouldBeEquivalentTo(expected);

                    //check subsequent tail is received
                    file.Append(CreateLines(101, 5));
                    scheduler.AdvanceBySeconds(10);
                    counter.Should().Be(3);

                }
            }
        }

        private string[] CreateLines(int start, int take)
        {
            return Enumerable.Range(start, take).Select(i => $"This is line number {i:00000000}").ToArray();
        }
    }
}