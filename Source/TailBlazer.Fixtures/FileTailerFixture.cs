using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using FluentAssertions;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    /*
        Putting a thread sleep into a test sucks. However the file system watcher 
        which ScanLineNumbers() is based on is async by nature and since
        it is built from old fashioned events there is no way to pass in a scheduler.
        If someone has a solution to eliminate Thread.Sleep crap, please let me know
    */
    public class FileTailerFixture
    {

        [Fact]
        public void AutoTail()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);

            var textMatch = Observable.Return((string)null);
            var autoTailer = Observable.Return(new ScrollRequest(10));


            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i =>i.ToString()).ToArray());

            using (var tailer = new FileTailer(info, textMatch, autoTailer))
            {

                tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(Enumerable.Range(91, 10));


                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => i.ToString()));

                Thread.Sleep(TimeSpan.FromSeconds(1));
                File.Delete(file);
                tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(Enumerable.Range(101, 10));
            }
        }

        [Fact]
        public void AutoTailWithFilter()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);

            var textMatch = Observable.Return((string)"1");
            var autoTailer = Observable.Return(new ScrollRequest(10));


            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());

            using (var tailer = new FileTailer(info, textMatch, autoTailer))
            {

                //lines which contain "1"
                var expectedLines = Enumerable.Range(1, 100)
                    .Select(i => i.ToString())
                    .Where(s => s.Contains("1"))
                    .Reverse()
                    .Take(10)
                    .Reverse()
                    .Select(int.Parse)
                    .ToArray();


                tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(expectedLines);


                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => i.ToString()));


                //lines which contain "1"
                expectedLines = Enumerable.Range(1, 110)
                    .Select(i => i.ToString())
                    .Where(s => s.Contains("1"))
                    .Reverse()
                    .Take(10)
                    .Reverse()
                    .Select(int.Parse)
                    .ToArray();

                Thread.Sleep(TimeSpan.FromSeconds(1));
                File.Delete(file);
                tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(expectedLines);
            }
        }


        [Fact]
        public void ScrollToSpecificLine()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);

            var textMatch = Observable.Return((string)null);


            var autoTailer =new ReplaySubject<ScrollRequest>(1);
            autoTailer.OnNext(new ScrollRequest(10,15));

            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());

            using (var tailer = new FileTailer(info, textMatch, autoTailer))
            {

                tailer.Lines.Items.Select(l => l.Number)
                    .ShouldAllBeEquivalentTo(Enumerable.Range(15, 10));

                autoTailer.OnNext(new ScrollRequest(15, 50));

             
                File.Delete(file);
                tailer.Lines.Items.Select(l => l.Number).ShouldAllBeEquivalentTo(Enumerable.Range(50, 15));
            }
         
        }
    }
}