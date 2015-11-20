using System;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{

        public class IndexLineNumbersFixture
    {

        [Fact]
        public void CanStreamFile()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            int[] result=new int[0];
            var scheduler = new TestScheduler();

            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => $"{i}").ToArray());

            using (info.WatchFile(TimeSpan.FromMilliseconds(1), scheduler).Index().Subscribe(x => result = x.Lines.Select((_,idx)=>idx+1).ToArray()))
            {
                scheduler.AdvanceByMilliSeconds(1);
                result.ShouldAllBeEquivalentTo(Enumerable.Range(1, 100));

                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => $"{i}"));
                scheduler.AdvanceByMilliSeconds(1);
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
            var scheduler = new TestScheduler();
            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => $"{i}").ToArray());

            //filter by odd numbers
            using (info.WatchFile(scheduler: scheduler).Index().Subscribe(x => result = x.Lines))
            {
                result.ShouldAllBeEquivalentTo(Enumerable.Range(1, 100).Where(i=>i % 2 == 1));

                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => $"{i}"));

                scheduler.AdvanceByMilliSeconds(250);
                File.Delete(file);
                result.ShouldAllBeEquivalentTo(Enumerable.Range(1, 110).Where(i => i % 2 == 1));
            }
        }
    }
}
