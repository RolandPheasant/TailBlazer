namespace TailBlazer.Domain.FileHandling
{
    public class FileSegment
    {
        public int Index { get; }
        public long Start { get;  }
        public long End { get;  }
        public FileSegmentType Type { get;  }

        public FileSegment(int index, long start, long end, FileSegmentType type)
        {
            Index = index;
            Start = start;
            End = end;
            Type = type;
        }
    }
}