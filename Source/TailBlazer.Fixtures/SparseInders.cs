using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class SparseIndexerFixture
    {
        [Fact]
        public void ProduceUncompressed()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            File.AppendAllLines(file, Enumerable.Range(1, 10000).Select(i => $"This is line number {i.ToString("00000000")}").ToArray());
            
            var indexer = new SparseIndexer(info);
            var start = (int)Math.Max(0, info.Length - 2000);
            var end = (int)info.Length;
            
            var tailIndex = indexer.Scan(start, end, 1);
            var startIndex = indexer.Scan(0, start, 10);

            var totalCount = tailIndex.LineCount + startIndex.LineCount;

            //there could be an overlapping index (the index collection will deal with this)
            totalCount.Should().Be(startIndex.End > start ? 10001 : 10000);
            
            //var averageLineLength = tailIndex.Size / tailIndex.LineCount;
            //var estimatedLines = info.Length / averageLineLength;
            //var actualLines = tailIndex.LineCount + startIndex.LineCount;

            File.Delete(file);
        }

        //[Fact]
        //public void TryItOnALArgeFile()
        //{

        //    var info = new FileInfo(@"U:\Work\2014\Dynamic.Trader\Trader.Client\bin\Debug\logs\logger.log");

        //    var indexer = new SparseIndexer(info);
        //    var start = (int)Math.Max(0, info.Length - 1000000);
        //    var end = (int)info.Length;

        //    var tailIndex = indexer.Scan(start, end, 1);
        //    var startIndex = indexer.Scan(0, start, 10);

        //    var totalCount = tailIndex.LineCount + startIndex.LineCount;

        //    //there could be an overlapping index (the index collection will deal with this)
        //    totalCount.Should().Be(startIndex.End > start ? 10001 : 10000);
        //}

    }
}
