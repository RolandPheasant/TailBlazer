using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class ReadLinesFixture
    {
        //[Fact()]
        public void FileWhereThereIsAProblemIndexing()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(@"U:\Downloads\InventairePerpetuel.txt");

            var indexer = new LineIndexer(info);
            var indexed = indexer.ReadToEnd().ToArray();
            var indicies = new LineIndicies(indexed, indexer.Encoding);



            //   var tail = indicies.GetTail(new ScrollRequest(1)).ToArray();
            var tail = indicies.GetIndicies( new ScrollRequest(1)).ToArray();

            Console.WriteLine(indicies.Lines.Last());
            var convertedLines = info.ReadLine(tail, (li, str) =>
            {
                Console.WriteLine(li.Size);
                Console.WriteLine(str.Length);
                return str;
            }, indicies.Encoding).ToArray();

        }



        //public class ArrayToLineEnum



        [Fact]
        public void ReadSpecificFileLines()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            File.AppendAllLines(file, Enumerable.Range(1, 100).Select(i => i.ToString()));

            var lines = info.ReadLines(new[] { 1, 2, 3, 10, 100, 105 });

            lines.Select(l=>l.Number).ShouldAllBeEquivalentTo(new[] { 1, 2, 3, 10, 100 });

            File.Delete(file);
        }
    }
}