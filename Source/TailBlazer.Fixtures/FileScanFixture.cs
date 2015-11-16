using System;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    /*
            Putting a thread sleep into a test sucks. However the file system watcher 
            which ScanLineNumbers() is based on is async by nature and since
            it is build from old fashioned events there is no way to pass in a scheduler.
            If someone has a solution to eliminate Thread.Sleep crap, please let me know
        */
        public class ScanLineNumberFixtures
    {

        [Fact]
        public void CanStreamFile()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            int[] result=new int[0];

            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => $"{i}").ToArray());

            using (info.WatchFile().ScanFile().Subscribe(x => result = x.MatchingLines.Select(l=>l.Line).ToArray()) )           {
                result.ShouldAllBeEquivalentTo(Enumerable.Range(1, 100));

                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => $"{i}"));

                Thread.Sleep(TimeSpan.FromSeconds(1));
                File.Delete(file);
                result.ShouldAllBeEquivalentTo(Enumerable.Range(1, 110));
       
            }
        }

        [Fact]
        public void CanStreamFileWithPredicate()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            int[] result = new int[0];

            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => $"{i}").ToArray());

            //filter by odd numbers
            using (info.WatchFile().ScanFile().Subscribe(x => result = x.MatchingLines.Select(l=>l.Line).ToArray()))
            {
                result.ShouldAllBeEquivalentTo(Enumerable.Range(1, 100).Where(i=>i % 2 == 1));

                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => $"{i}"));

                Thread.Sleep(TimeSpan.FromSeconds(1));
                File.Delete(file);
                result.ShouldAllBeEquivalentTo(Enumerable.Range(1, 110).Where(i => i % 2 == 1));
            }
        }
    }
}
