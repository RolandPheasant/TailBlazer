using System;
using System.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class AutoTailFixture
    {
        [Fact]
        public void TailsLatestValuesOnly()
       {
            var scheduler = new TestScheduler();
            var size = new BehaviorSubject<int>(10);

            using (var file = new TestFile())
            {
                file.Append(CreateLines(1, 100));

                var autoTailer = new AutoTail(file.Info.WatchFile(scheduler: scheduler).Index(), size);

                AutoTailResponse result = null;
                int counter = 0;
                using (autoTailer.Tail().Subscribe(x=> { result = x; counter++; }))
                {
                    //check that intial values are loaded
                    scheduler.AdvanceBySeconds(250);
                    counter.Should().Be(1);
                    result.Count.Should().Be(10);
                    var expected = CreateLines(91, 10);
                    result.Lines.Join().ShouldBeEquivalentTo(expected.Join());

                    file.Append(CreateLines(101, 5));
                    scheduler.AdvanceBySeconds(1);
                    counter.Should().Be(2);
                    result.Count.Should().Be(5);
                    expected = CreateLines(101, 5);
                    result.Lines.Join().ShouldBeEquivalentTo(expected.Join());
                }
            }
        }

        [Fact]
        public void TailsLatestValuesOnly_ForFilteredValues()
        {
            var scheduler = new TestScheduler();
            var size = new BehaviorSubject<int>(10);

            using (var file = new TestFile("TailsLatestValuesOnly_ForFilteredValues"))
            {
                file.Append(CreateLines(1, 100));

                var autoTailer = new AutoTail(file.Info.WatchFile(scheduler: scheduler).Search(str => str.Contains("9")),size);

                AutoTailResponse result = null;
                int counter = 0;
                using (autoTailer.Tail().Subscribe(x => { result = x; counter++; }))
                {
                    //check that intial values are loaded
                    scheduler.AdvanceBySeconds(1);
                     counter.Should().Be(1);
                    result.Count.Should().Be(10);
                    var expected = CreateLines(90, 10);
                    result.Lines.Join().ShouldBeEquivalentTo(expected.Join());

                    file.Append(CreateLines(101, 10));
                    scheduler.AdvanceBySeconds(1);
                    counter.Should().Be(2);
                    result.Count.Should().Be(1);
                    expected = CreateLines(109, 1);
                    result.Lines.Join().ShouldBeEquivalentTo(expected.Join());
                }
            }
        }

        private string[] CreateLines(int start, int take)
        {
            return Enumerable.Range(start, take).Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
        }
    }
}
