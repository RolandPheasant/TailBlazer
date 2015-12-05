using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public class SparseIndex
    {
        public long Start { get; }
        public long End { get; }
        public ImmutableList<long> Indicies { get; }

 
        public int Compression { get; }
        public int LineCount { get; }
        public int IndexCount => Indicies.Count;

        public IndexType Type { get; }

        public long Size => End - Start;

        public DateTime TimeStamp { get; } = DateTime.Now;

        public SparseIndex(long start, long end, long[] indicies, int compression, int lineCount, IndexType type)
        {
            Start = start;
            End = end;
            Indicies = new ImmutableList<long>(indicies);
            Compression = compression;
            LineCount = lineCount;
            Type = type;
        }
        

        public SparseIndex(long start, long end,  int compression, int lineCount, IndexType type)
        {
            Start = start;
            End = end;
            Indicies = new ImmutableList<long>();
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
            Indicies = previous.Indicies.Add(latest.Indicies);
        }
    }
}