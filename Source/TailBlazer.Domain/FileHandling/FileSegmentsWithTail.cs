using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
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