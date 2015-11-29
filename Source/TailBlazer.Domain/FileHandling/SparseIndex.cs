using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public enum SpareIndexType
    {
        Tail,
        Page
    }

    public class SparseIndex
    {
        public int Start { get; }
        public int End { get; }
        public int[] Indicies { get; }
        public int Compression { get; }
        public int LineCount { get; }
        public int IndexCount => Indicies.Length;

        public SpareIndexType Type { get; }

        public int Size => End - Start;

        public DateTime TimeStamp { get; } = DateTime.Now;

        public SparseIndex(int start, int end, int[] indicies, int compression, int lineCount, SpareIndexType type)
        {
            Start = start;
            End = end;
            Indicies = indicies;
            Compression = compression;
            LineCount = lineCount;
            Type = type;
        }
        

        public SparseIndex(int start, int end,  int compression, int lineCount, SpareIndexType type)
        {
            Start = start;
            End = end;
            Indicies = new int [0];
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
            var items = new int[previous.Indicies.Length + latest.Indicies.Length];
            previous.Indicies.CopyTo(items, 0);
            latest.Indicies.CopyTo(items, previous.Indicies.Length);
            Indicies = items;
        }
    }
}