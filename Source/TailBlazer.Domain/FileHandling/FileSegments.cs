using System;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSegments
    {
        public FileSegment[] Segments { get;  }
        public long TailStartsAt { get;  }
        public int Count { get;  }
        public FileSegmentChangedReason Reason { get; }

        public FileSegment Tail => Segments[Count - 1];

        public long FileLength => Tail.End;

        public FileSegments(FileSegment[] segments)
        {
            if (segments.Length == 0)
                throw new ArgumentException("Argument is empty collection", nameof(segments));

            Segments = segments;
            TailStartsAt = segments.Max(fs => fs.End);
            Count = Segments.Length;
            Reason = FileSegmentChangedReason.Loaded;
        }

        public FileSegments(long newLength, FileSegments previous)
        {
            Reason = FileSegmentChangedReason.Tailed;
            var last = previous.Tail;
            Segments = previous.Segments;
            Count = Segments.Length;
            Segments[Count-1] = new FileSegment(last, newLength);
            TailStartsAt = last.End;

        }
    }
}