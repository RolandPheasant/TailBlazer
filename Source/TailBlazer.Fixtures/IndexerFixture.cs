using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class IndexerFixture
    {
        [Fact]
        public void CanReadIndiciesBack_SmallFile()
        {
            var pulse = new Subject<Unit>();
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Append(Enumerable.Range(1, 100).Select(i => $"This is line number {i.ToString("00000000")}").ToArray());

                using (var indexer = new Indexer(file.Info.WatchFile(pulse).WithSegments(), scheduler: scheduler))
                {
                    IndexCollection result = null;
                    using (indexer.Result.Subscribe(indicies => result = indicies))

                    {
                        pulse.Once();

                        var head = result.ReadLines(new ScrollRequest(10, 0));
                        var headText = head.Select(l => l.Text).ToArray();
                        var headExpected = Enumerable.Range(1, 10).Select(i => $"This is line number {i.ToString("00000000")}");
                        headText.Should().BeEquivalentTo(headExpected);

                        var tail = result.ReadLines(new ScrollRequest(10));
                        var tailText = tail.Select(l => l.Text).ToArray();
                        var tailExpected = Enumerable.Range(91, 10).Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
                        tailText.Should().BeEquivalentTo(tailExpected);

                        var mid = result.ReadLines(new ScrollRequest(10, 20));
                        var midText = mid.Select(l => l.Text).ToArray();
                        var midExpected = Enumerable.Range(21, 10).Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
                        midText.Should().BeEquivalentTo(midExpected);
                    }
                }
            }
        }

        [Fact]
        public void CanReadIndiciesBack_LargeFile()
        { 
            var pulse = new Subject<Unit>();
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Append(Enumerable.Range(1, 10000).Select(i => $"This is line number {i.ToString("00000000")}").ToArray());

                using (var indexer = new Indexer(file.Info.WatchFile(pulse).WithSegments(), tailSize: 1000,scheduler: scheduler))
                {

                    IndexCollection result = null;
                    using (indexer.Result.Subscribe(indicies => result = indicies))

                    {
                        //start off the head scanner
                        scheduler.AdvanceBy(1);

                        var head = result.ReadLines(new ScrollRequest(10, 0));
                        var headText =head.Select(l=>l.Text).ToArray();
                        var headExpected =Enumerable.Range(1, 10).Select(i => $"This is line number {i.ToString("00000000")}");
                        headText.Should().BeEquivalentTo(headExpected);

                        var tail = result.ReadLines(new ScrollRequest(10));
                        var tailText = tail.Select(l => l.Text).ToArray();
                        var tailExpected =Enumerable.Range(9991, 10).Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
                        tailText.Should().BeEquivalentTo(tailExpected);

                        var mid = result.ReadLines(new ScrollRequest(10, 100));
                        var midText = mid.Select(l => l.Text).ToArray();
                        var midExpected =Enumerable.Range(101, 10).Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
                        midText.Should().BeEquivalentTo(midExpected);
                    }
                }
            }
        }

        [Fact]
        public void CanProduceIndices()
        {
            var pulse = new Subject<Unit>();
            var scheduler = new TestScheduler();
            using (var file = new TestFile())
            {

                using (var indexer = new Indexer(file.Info.WatchFile(pulse).WithSegments(), tailSize: 1000,
                        scheduler: scheduler))
                {
                    var lines = Enumerable.Range(1, 10000)
                        .Select(i => $"This is line number {i.ToString("00000000")}")
                        .ToArray();
                    file.Append(lines);
                   // scheduler.AdvanceBy(1);
                    pulse.Once();
                    IndexCollection result = null;
                    using (indexer.Result.Subscribe(indicies => result = indicies))
                    {
                        //start off the head scanner
                        scheduler.AdvanceBy(1);
                        var totalCount = result.Count;

                        //there could be an overlapping index (the index collection will deal with this)
                        totalCount.Should().Be(10000);
                    }
                }
            }
        }


        [Fact]
        public void WillAutoTail()
        {
            var pulse = new Subject<Unit>();
            var scheduler = new TestScheduler();
            using (var file = new TestFile())
            {

                using (var indexer = new Indexer(file.Info.WatchFile(pulse).WithSegments(), tailSize: 1000,
                        scheduler: scheduler))
                {
                    file.Append(Enumerable.Range(1, 10000)
                            .Select(i => $"This is line number {i.ToString("00000000")}")
                            .ToArray());
                    pulse.Once();
                    IndexCollection result = null;
                    using (indexer.Result.Subscribe(indicies => result = indicies))
                    {
                        scheduler.AdvanceBy(1);
                        result.Count.Should().Be(10000);
                        file.Append(Enumerable.Range(10001, 10).Select(i => $"This is line number {i.ToString("00000000")}").ToArray());
                        pulse.OnNext(Unit.Default);
                        result.Count.Should().Be(10010);
                    }
                }
            }

        }
    }
}
