namespace TailBlazer.Domain.FileHandling
{

    public enum FileSegmentType
    {
        Head,
        Body,
        Tail
    }

    public class FileSegment
    {
        public int Start { get;  }
        public int End { get;  }
        public FileSegmentType Type { get;  }

        public FileSegment(int start, int end, FileSegmentType type)
        {
            Start = start;
            End = end;
            Type = type;
        }
    }

    public class FileSegmenter
    {
        //dynamically split the file into sements according to the size of file.
        //as the file size changes, allow these segments to dynamically resize


        public FileSegmenter()
        {
        }
    }
}