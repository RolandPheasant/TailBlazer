using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;

namespace TailBlazer.Domain.FileHandling
{

    public class FileSegments
    {
        public FileInfo Info { get;  }
        public FileSegment[] Segments { get;  }
        public long TailStartsAt { get;  }
        public int Count { get;  }
        public FileSegmentChangedReason Reason { get; }

        public FileSegment Tail => Segments[Count - 1];

        public long FileLength => Tail.End;

        public FileSegments(FileInfo fileInfo, FileSegment[] segments)
        {
            if (segments.Length == 0)
                throw new ArgumentException("Argument is empty collection", nameof(segments));

            Info = fileInfo;
            Segments = segments;
            TailStartsAt = segments.Max(fs => fs.End);
            Count = Segments.Length;
            Reason = FileSegmentChangedReason.Loaded;
        }

        public FileSegments(long newLength, FileSegments previous)
        {
            Reason = FileSegmentChangedReason.Tailed;

            Info = previous.Info;

            var last = previous.Tail;
            Segments = previous.Segments;
          
            Segments[Count-1] = new FileSegment(last, newLength);
            Count = Segments.Length;
            TailStartsAt = last.End;

        }
    }
}