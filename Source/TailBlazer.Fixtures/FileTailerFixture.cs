using System;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using Xunit;

namespace TailBlazer.Fixtures
{

    public class FileTailerFixture
    {

      //  [Fact]
        //public void AutoTail()
        //{
        //    var scheduler  = new TestScheduler();
        //    var textMatch = Observable.Return(FileSearchResult.None);
        //    var autoTailer = Observable.Return(new ScrollRequest(10));

        //    using (var file = new TestFile())
        //    {
        //        file.Append( Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());

        //        using (var tailer = new FileTailer(file.Info, textMatch, autoTailer, new NullLogger(), scheduler))
        //        {
        //            scheduler.AdvanceByMilliSeconds(250);
        //            tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(Enumerable.Range(91, 10));
        //            file.Append( Enumerable.Range(101, 10).Select(i => i.ToString()));

        //            scheduler.AdvanceByMilliSeconds(250);
        //            tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(Enumerable.Range(101, 10));
        //        }
        //    }
        //}

        //[Fact]
        //public void WillNotApplyFilterWhenTextIsLessThan3Character()
        //{
        //    var file = Path.GetTempFileName();
        //    var info = new FileInfo(file);
        //    var scheduler = new TestScheduler();
        //    var textMatch = Observable.Return((string)"1");
        //    var autoTailer = Observable.Return(new ScrollRequest(10));


        //    File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());

        //    using (var tailer = new FileTailer(info, textMatch, autoTailer, new NullLogger(), scheduler))
        //    {

        //        //lines which contain "1"
        //        int[] expectedLines = Enumerable.Range(91, 10)
        //            .Select(i => i.ToString())
        //            .Select(int.Parse)
        //            .ToArray();

        //        scheduler.AdvanceBySeconds(1);

        //        tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(expectedLines);

        //        File.Delete(file);
        //        tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(expectedLines);
        //    }
        //}

        [Fact]
        public void AutoTailWithFilter()
        {

            var scheduler = new TestScheduler();
            var autoTailer = Observable.Return(new ScrollRequest(10));

            bool Predicate(string s) => s.Contains("odd");
            using (var file = new TestFile())
            {

                file.Append(Enumerable.Range(1, 100).Select(i => i%2 == 1 ? $"{i} is an odd number" : $"{i} is an even number").ToArray());
                var search = file.Info.Search(Predicate,  scheduler);

                using (var tailer = new LineScroller(file.Info, search, autoTailer, new NullLogger(), scheduler))
                {

                    //lines which contain "1"
                    var expectedLines = Enumerable.Range(1, 100)
                        .Select(i => i%2 == 1 ? $"{i} is an odd number" : $"{i} is an even number")
                        .Where(s => s.Contains("odd"))
                        .Reverse()
                        .Take(10)
                        .Reverse()
                        .ToArray();

                    scheduler.AdvanceBySeconds(1);

                    tailer.Lines.Items.Select(l => l.Text).ShouldAllBeEquivalentTo(expectedLines);


                    file.Append( Enumerable.Range(101, 10).Select(i => i%2 == 1 ? $"{i} is an odd number" : $"{i} is an even number").ToArray());

                    scheduler.AdvanceBySeconds(1);

                    expectedLines = Enumerable.Range(1, 110)
                        .Select(i => i%2 == 1 ? $"{i} is an odd number" : $"{i} is an even number")
                        .Where(s => s.Contains("odd"))
                        .Reverse()
                        .Take(10)
                        .Reverse()
                        .ToArray();


                    scheduler.AdvanceBySeconds(1);


                    tailer.Lines.Items.Select(l => l.Text).ShouldAllBeEquivalentTo(expectedLines);
                }
            }
        }


        //[Fact]
        //public void ScrollToSpecificLine()
        //{
        //    var file = Path.GetTempFileName();
        //    var info = new FileInfo(file);
        //    var textMatch = Observable.Return((string)null);
        //    var scheduler = new TestScheduler();
        //    var autoTailer =new ReplaySubject<ScrollRequest>(1);
        //    autoTailer.OnNext(new ScrollRequest(10,14));

        //    File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());

        //    using (var tailer = new FileTailer(info, textMatch, autoTailer,new NullLogger(), scheduler))
        //    {
        //        scheduler.AdvanceByMilliSeconds(251);
        //        tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(Enumerable.Range(15, 10));

        //        autoTailer.OnNext(new ScrollRequest(15, 49));


        //        File.Delete(file);
        //        scheduler.AdvanceByMilliSeconds(250);
        //        tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(Enumerable.Range(50, 15));
        //    }

        //}

        //[Fact]
        //public void AnEmptyFileDoesNotBomb()
        //{
        //    var file = Path.GetTempFileName();
        //    var info = new FileInfo(file);
        //    var scheduler = new TestScheduler();
        //    var textMatch = Observable.Return((string)null);
        //    var autoTailer = Observable.Return(new ScrollRequest(10));

        //    using (var tailer = new FileTailer(info, textMatch, autoTailer, new NullLogger(), scheduler))
        //    {
        //        scheduler.AdvanceByMilliSeconds(1);
        //        File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());
        //        scheduler.AdvanceBySeconds(1);
        //        tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(Enumerable.Range(91, 10));
        //    }
        //}
    }
}