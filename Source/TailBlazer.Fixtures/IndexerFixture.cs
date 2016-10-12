using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class IndexerFixture
    {
        [Fact]
        public void CanReadIndiciesBack_SmallFile()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Append(1, 100);
                var indexer = new FileIndexer(file.Info.WatchFile(scheduler: scheduler).SegmentWithReport());

                FileIndexCollection result = null;
                using (indexer.Result.Subscribe(indicies => result = indicies))

                {
                             scheduler.AdvanceByMilliSeconds(500);

                    var head = result.ReadLines(new ScrollRequest(10, 0)).ToArray();
                    var headText = head.Select(l => l.Text).ToArray();
                    var headExpected = Enumerable.Range(1, 10).Select(i => $"This is line number {i.ToString("00000000")}");
                    headText.ShouldAllBeEquivalentTo(headExpected);

                    var tail = result.ReadLines(new ScrollRequest(10)).ToArray();
                    var tailText = tail.Select(l => l.Text).ToArray();
                    var tailExpected = Enumerable.Range(91, 10).Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
                    tailText.ShouldAllBeEquivalentTo(tailExpected);

                    var mid = result.ReadLines(new ScrollRequest(10, 20)).ToArray();
                    var midText = mid.Select(l => l.Text).ToArray();
                    var midExpected = Enumerable.Range(20, 10).Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
                    midText.ShouldAllBeEquivalentTo(midExpected);
                }
            }
            
        }

        [Fact]
        public void CanReadIndiciesBack_LargeFile()
        { 
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Append(1, 10000);
                var indexer = new FileIndexer(file.Info.WatchFile(scheduler: scheduler).SegmentWithReport(1000));

                    FileIndexCollection result = null;
                    using (indexer.Result.Subscribe(indicies => result = indicies))

                    {
                        //start off the head scanner
                        scheduler.AdvanceByMilliSeconds(500);

                        var head = result.ReadLines(new ScrollRequest(10, 0)).ToArray(); 
                        var headText =head.Select(l=>l.Text).ToArray();
                        var headExpected =Enumerable.Range(1, 10).Select(i => $"This is line number {i:00000000}");
                        headText.ShouldAllBeEquivalentTo(headExpected);

                        var tail = result.ReadLines(new ScrollRequest(10)).ToArray();
                        var tailText = tail.Select(l => l.Text).ToArray();
                        var tailExpected =Enumerable.Range(9991, 10).Select(i => $"This is line number {i:00000000}").ToArray();
                        tailText.ShouldAllBeEquivalentTo(tailExpected);

                        var mid = result.ReadLines(new ScrollRequest(10, 100)).ToArray(); 
                        var midText = mid.Select(l => l.Text).ToArray();
                        var midExpected =Enumerable.Range(101, 10).Select(i => $"This is line number {i:00000000}").ToArray();
                        midText.ShouldAllBeEquivalentTo(midExpected);
                    }
                }
            
        }

        [Fact]
        public void CanProduceIndices()
        {
            var scheduler = new TestScheduler();
            using (var file = new TestFile())
            {
                var indexer = new FileIndexer(file.Info.WatchFile(scheduler: scheduler).SegmentWithReport(1000));

                var lines =  file.Append(1,10000);
                FileIndexCollection result = null;
                using (indexer.Result.Subscribe(indicies => result = indicies))
                {
                    //start off the head scanner
                    scheduler.AdvanceByMilliSeconds(250);
                    var totalCount = result.Count;

                    //there could be an overlapping index (the index collection will deal with this)
                    totalCount.Should().Be(10000);
                }
            }
        }


        [Fact]
        public void WillAutoTail()
        {
            var scheduler = new TestScheduler();
            using (var file = new TestFile())
            {
                var indexer = new FileIndexer(file.Info.WatchFile(scheduler: scheduler).SegmentWithReport(1000));
                
                file.Append(1, 10000);

                FileIndexCollection result = null;
                using (indexer.Result.Subscribe(indicies => result = indicies))
                {
                    scheduler.AdvanceByMilliSeconds(500);
                    result.Count.Should().Be(10000);
                    file.Append(10001, 10);
                    scheduler.AdvanceByMilliSeconds(250);
                    result.Count.Should().Be(10010);
                }
            }

        }

        [Fact]
        public void EmptyFileHandling()
        {
            var scheduler = new TestScheduler();
            using (var file = new TestFile())
            {
                var indexer = new FileIndexer(file.Info.WatchFile(scheduler: scheduler).SegmentWithReport(1000));

                file.Append(1, 10000);

                FileIndexCollection result = null;
                using (indexer.Result.Subscribe(indicies => result = indicies))
                {
                    scheduler.AdvanceByMilliSeconds(500);
                    result.Count.Should().Be(10000);
                    file.Append(10001, 10);
                    scheduler.AdvanceByMilliSeconds(250);
                    result.Count.Should().Be(10010);
                }
                
            }

        }
    }
}
