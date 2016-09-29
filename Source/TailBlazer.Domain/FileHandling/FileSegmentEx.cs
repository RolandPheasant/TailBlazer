using System;
using System.Reactive.Linq;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileSegmentEx
    {
        public static IObservable<FileSegmentCollection> WithSegments(this IObservable<FileNotification> source, int initialTail= 100000)
        {
            var shared = source.Replay(1).RefCount();

            return Observable.Create<FileSegmentCollection>(observer =>
            {
                var filtered = source.Where(f=>f.Exists);
                var segmenter = new FileSegmenter(filtered, initialTail);
                return segmenter.Segments.SubscribeSafe(observer);
            })
            .TakeUntil(shared.Where(f => !f.Exists));
        }

        public static IObservable<FileTailInfo> Tail([NotNull] this IObservable<FileSegmentCollection> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new FileTailReader(source).Tail();
        }
    }
}