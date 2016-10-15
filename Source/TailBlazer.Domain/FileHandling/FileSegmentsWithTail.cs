using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    /*
     1. Change TailInfo to latest tail info 
     2. Accumulate tail to include an aggregated tail [need to determine cache size]
     3. Change LineScroller so that it tails from memory rather than by going back to file i.e Tail mode / Scroll mode
     4. Determine what a good cache size is 1000?
    */

    public class FileSegmentsWithTail
    {
        public TailInfo TailInfo { get; }
        public FileSegmentCollection Segments { get; }

        public FileSegmentsWithTail([NotNull] FileSegmentCollection segments, [NotNull] TailInfo tailInfo)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            if (tailInfo == null) throw new ArgumentNullException(nameof(tailInfo));
            TailInfo = tailInfo;
            Segments = segments;
        }
    }

    public class FileSegmentReport
    {
        public TailInfo TailInfo { get; }
        public FileChanges Changes { get;  }
        public FileSegmentCollection Segments { get; }

        public FileSegmentReport([NotNull] FileSegmentCollection segments, [NotNull] TailInfo tailInfo, FileChanges changes)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            if (tailInfo == null) throw new ArgumentNullException(nameof(tailInfo));
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            if (!changes.Exists)
            {
                TailInfo = TailInfo.Empty;

            }
            else
            {
                TailInfo = tailInfo;
            }

           // TailInfo = !Changes.Exists ? TailInfo.Empty : tailInfo;
            Changes = changes;
            Segments = segments;
        }
    }
}