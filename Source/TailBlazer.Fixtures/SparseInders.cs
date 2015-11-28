using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    
    public class SparseIndex
    {
        public int Start { get; }
        public int End { get; }
        public int[] Indicies { get; }
        public int Compression { get; }
        public int LineCount { get;  }
        public int IndexCount => Indicies.Length;
        public int Size => Indicies[IndexCount-1] - Start;

        public SparseIndex(int start, int end, int[] indicies, int compression, int lineCount)
        {
            Start = start;
            End = end;
            Indicies = indicies;
            Compression = compression;
            LineCount = lineCount;
        }
    }

    public class SparseIndexer
    {


        public IObservable<SparseIndex> IndexChanged { get; }
        public Encoding Encoding { get; }
        public FileInfo Info { get; }

        public SparseIndexer([NotNull] FileInfo info, Encoding encoding = null)
        {
            Info = info;
            if (info == null) throw new ArgumentNullException(nameof(info));

            Encoding = encoding ?? info.GetEncoding();

            //1. Get  full length of file

            //2. Scan tail (say 1 meg)

            //3. Can tail (say 1 meg)
            //report data back with an estimate of the number of Indicies

            //4. Continue with indexing whilst feedig back progress

            //take last MG for head
            var start = Math.Max(0,info.Length - 1000000);


            var end = info.Length;

        }


        public SparseIndex Scan(int start, int end, int compression)
        {
            int count = 0;


            int lastPosition = 0;
            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read,FileShare.Delete | FileShare.ReadWrite))
            {
                int[] lines;
                using (var reader = new StreamReaderExtended(stream, Encoding, false))
                {
                    reader.BaseStream.Seek(start, SeekOrigin.Begin);

                    lines = reader.ScanLines(compression, i => i, (line, position) =>
                    {
                        lastPosition = position;
                        count++;

                        return lastPosition >= end ;

                    }).ToArray();
                }


                if (lastPosition > end)
                { 
                    //we have an overlapping line [must remove the last one from the head]

                  //   lines = lines.Take(count - 1).ToArray();
                }
                return new SparseIndex(start, lastPosition, lines, compression, count);
            }
        }
    }

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
           // var headIndex = indexer.Scan(0, end, 1);
            var startIndex = indexer.Scan(0, start, 10);

            var totalCount = tailIndex.LineCount + startIndex.LineCount;
            //there could be an overlapping index (the index collection will deal with this)
            totalCount.Should().Be(startIndex.End > start ? 100001 : 100000);


            //var averageLineLength = tailIndex.Size / tailIndex.LineCount;
            //var estimatedLines = info.Length / averageLineLength;
            //var actualLines = tailIndex.LineCount + startIndex.LineCount;

            File.Delete(file);
        }

    }
}
