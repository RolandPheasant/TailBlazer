using System;
using System.Reactive.Disposables;
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
                var segments = new FileSegmenter(filtered, initialTail).Segments;
                return segments.SubscribeSafe(observer);
            })
            .TakeUntil(shared.Where(f => !f.Exists));
        }

        public static IObservable<TailInfo> Tail([NotNull] this IObservable<FileSegmentCollection> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new FileTailReader(source).Tail();
        }

        public static IObservable<FileSegmentsWithTail> WithTail([NotNull] this IObservable<FileSegmentCollection> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return Observable.Create<FileSegmentsWithTail>(observer =>
            {
                var shared = source.Publish();
                var fileTail = shared.Tail();
                var segments = shared.CombineLatest(fileTail, (segment, tail) => new FileSegmentsWithTail(segment, tail));
                return new CompositeDisposable(segments.SubscribeSafe(observer), shared.Connect());
            });
        }

    }
}