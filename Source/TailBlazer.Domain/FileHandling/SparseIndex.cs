using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class SparseIndex
    {
        public long Start { get; }
        public long End { get; }
        public long[] Indicies { get; }
        public int Compression { get; }
        public int LineCount { get; }
        public int IndexCount => Indicies.Length;

        public IndexType Type { get; }

        public long Size => End - Start;

        public DateTime TimeStamp { get; } = DateTime.Now;

        public SparseIndex(long start, long end, long[] indicies, int compression, int lineCount, IndexType type)
        {
            Start = start;
            End = end;
            Indicies = indicies;
            Compression = compression;
            LineCount = lineCount;
            Type = type;
        }
        

        public SparseIndex(long start, long end,  int compression, int lineCount, IndexType type)
        {
            Start = start;
            End = end;
            Indicies = new long[0];
            Compression = compression;
            LineCount = lineCount;
            Type = type;
        }

        public SparseIndex([NotNull] SparseIndex latest, SparseIndex previous)
        {
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            if (previous == null) throw new ArgumentNullException(nameof(previous));

            Start = previous.Start;
            End = latest.End;
            Compression = latest.Compression;
            LineCount = latest.LineCount + previous.LineCount;
            Type = latest.Type;

            //combine latest arrays
            var items = new long[previous.Indicies.Length + latest.Indicies.Length];
            previous.Indicies.CopyTo(items, 0);
            latest.Indicies.CopyTo(items, previous.Indicies.Length);
            Indicies = items;
        }
    }
}