using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using FluentAssertions;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{

  public class IndexLineNumbersFixture
    {

        [Fact]
        public void IndexLines()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            int[] result=new int[0];

            var subject = new Subject<Unit>();

            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => $"{i}").ToArray());

            using (info.WatchFile(subject).Index().Subscribe(x => result = x.Lines.Select((_,idx)=>idx+1).ToArray()))
            {
        
                result.ShouldAllBeEquivalentTo(Enumerable.Range(1, 100));

                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => $"{i}"));
                subject.OnNext(Unit.Default);
                File.Delete(file);
                result.ShouldAllBeEquivalentTo(Enumerable.Range(1, 110));

            }
        }

        [Fact]
        public void MatchLines()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            int[] result = new int[0];

            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => $"{i}").ToArray());

            var subject = new Subject<Unit>();

            //filter by odd numbers
            using (info.WatchFile(subject).Match(i => int.Parse(i) % 2 == 1).Subscribe(x => result = x.Lines))
            {
                var expected = Enumerable.Range(1, 100).Where(i => i%2 == 1).Select(i=>i-1).ToArray();
                result.ShouldAllBeEquivalentTo(expected);

                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => $"{i}"));
                subject.OnNext(Unit.Default);
                File.Delete(file);

                expected = Enumerable.Range(1, 110).Where(i => i % 2 == 1).Select(i => i - 1).ToArray();
                result.ShouldAllBeEquivalentTo(expected);
            }
        }
    }
}
