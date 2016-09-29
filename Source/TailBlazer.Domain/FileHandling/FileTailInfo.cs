using System;

namespace TailBlazer.Domain.FileHandling
{
    public class FileTailInfo
    {
        public Line[] Tail { get; }
        public int Count => Tail.Length;
        public long Start { get; }
        public long End { get; }
        public long Size => End - Start;

        public DateTime DateTime { get; }

        public static readonly FileTailInfo Empty = new FileTailInfo(new Line[0]);

        public FileTailInfo(Line[] tail)
        {
            DateTime = DateTime.UtcNow;
            Tail = tail;
            Start = tail.Length == 0 ? 0 : tail[0].LineInfo.Start;
            End = tail.Length == 0 ? 0 : tail[tail.Length-1].LineInfo.End;
        }
    }
}