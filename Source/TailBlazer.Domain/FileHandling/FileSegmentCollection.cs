using System;
using System.IO;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSegmentCollection
    {
        public FileInfo Info { get;  }
        public FileSegment[] Segments { get;  }
        public long TailStartsAt { get;  }
        public int Count { get;  }
        public FileSegmentChangedReason Reason { get; }
        public FileSegment Tail => Segments[Count - 1];
        public long FileLength => Tail.End;

        public FileSegmentCollection(FileInfo fileInfo, FileSegment[] segments)
        {
            if (segments.Length == 0)
                throw new ArgumentException("Argument is empty collection", nameof(segments));

            Info = fileInfo;
            Segments = segments;
            TailStartsAt = segments.Max(fs => fs.End);
            Count = Segments.Length;
            Reason = FileSegmentChangedReason.Loaded;
        }

        public FileSegmentCollection(long newLength, FileSegmentCollection previous)
        {
            //All this assumes it is the tail which has changed, but that may not be so
            Reason = FileSegmentChangedReason.Tailed;
            Info = previous.Info;

            var last = previous.Tail;
            TailStartsAt = last.End;

            var segments = previous.Segments;
            segments[segments.Length-1] = new FileSegment(last, newLength);
            Segments = segments;
            Count = Segments.Length;
        }
    }
}