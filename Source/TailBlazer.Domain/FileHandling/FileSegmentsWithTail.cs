using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    /*
     1. Change TailInfo to latest tail info 
     2. Accumulate tail to include an aggregated tail [need to determine cache size]
     3. Change LineScroller so that it tails from memory rather than by going back to file i.e Tail mode / Scroll mode
    */

    public class FileSegmentsWithTail
    {
        public FileTailInfo TailInfo { get; }
        public FileSegmentCollection Segments { get; }

        public FileSegmentsWithTail([NotNull] FileSegmentCollection segments, [NotNull] FileTailInfo tailInfo)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            if (tailInfo == null) throw new ArgumentNullException(nameof(tailInfo));
            TailInfo = tailInfo;
            Segments = segments;
        }
    }
}