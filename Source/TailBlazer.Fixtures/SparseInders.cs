using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class SparseIndexerFixture
    {
        [Fact]
        public async void ProduceUncompressed()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            File.AppendAllLines(file, Enumerable.Range(1, 10000).Select(i => $"This is line number {i.ToString("00000000")}").ToArray());
            
            var indexer = new SparseIndexer(info);
            var start = (int)Math.Max(0, info.Length - 2000);
            var end = (int)info.Length;

            var tailIndex = await indexer.ScanAsync(start, end, 1);
            var startIndex = await indexer.ScanAsync(0, start, 10);

              var totalCount = tailIndex.LineCount + startIndex.LineCount;

            //there could be an overlapping index (the index collection will deal with this)
            totalCount.Should().Be(startIndex.End > start ? 10001 : 10000);

            //var averageLineLength = tailIndex.Size / tailIndex.LineCount;
            //var estimatedLines = info.Length / averageLineLength;
            //var actualLines = tailIndex.LineCount + startIndex.LineCount;

            File.Delete(file);
        }

        [Fact]
        public async void WillAutoTail()
        {
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            var scheduler = new TestScheduler();
            File.AppendAllLines(file, Enumerable.Range(1, 10000).Select(i => $"This is line number {i.ToString("00000000")}").ToArray());

            var indexer = new SparseIndexer(info,tailSize:1000, scheduler: scheduler);
            //var start = (int)Math.Max(0, info.Length - 2000);
            //var end = (int)info.Length;

            //var tailIndex = await indexer.ScanAsync(start, end, 1);
            //var startIndex = await indexer.ScanAsync(0, start, 10);
            scheduler.AdvanceBy(1);

            var totalCount = indexer.Indicies.Items.Sum(si=>si.LineCount);

            //there could be an overlapping index (the index collection will deal with this)
            totalCount.Should().Be( 10001 );

            //var averageLineLength = tailIndex.Size / tailIndex.LineCount;
            //var estimatedLines = info.Length / averageLineLength;
            //var actualLines = tailIndex.LineCount + startIndex.LineCount;

            File.Delete(file);
        }
    }
}
