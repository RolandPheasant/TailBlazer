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
            return new FileSegmenter(source, initialTail).Segments;
        }

        public static IObservable<FileSegmentCollection> WithSegments(this IObservable<FileChanges> source, int initialTail = 100000)
        {
            return new FileSegmenter(source, initialTail).Segments;
        }

        public static IObservable<TailInfo> Tail([NotNull] this IObservable<FileSegmentCollection> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new FileTailReader(source).Tail();
        }

        public static IObservable<FileSegmentReport> SegmentWithReport([NotNull] this IObservable<FileNotification> source, int initialTail = 100000)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return Observable.Create<FileSegmentReport>(observer =>
            {
                var changes = source.MonitorChanges().DistinctUntilChanged().Publish();
                var segments = changes.WithSegments(initialTail).DistinctUntilChanged().Publish();

              //  var zipped = changes.Zip(segments, (c, s) => new );

                var fileTail = segments.Tail().DistinctUntilChanged();
                var combined = segments.CombineLatest(fileTail, changes, (segment, tail, change) => new FileSegmentReport(segment, tail, change));

                return new CompositeDisposable(combined.SubscribeSafe(observer), segments.Connect(), changes.Connect());
            });
        }

        public static IObservable<FileSegmentReport> SegmentWithReport([NotNull] this IObservable<FileChanges> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return Observable.Create<FileSegmentReport>(observer =>
            {
                var changes = source.Publish();
                var segments = changes.WithSegments().Publish();
                var fileTail = segments.Tail();
                var combined = segments.CombineLatest(fileTail, changes, (segment, tail, change) => new FileSegmentReport(segment, tail, change));

                return new CompositeDisposable(combined.SubscribeSafe(observer), segments.Connect(), changes.Connect());
            });
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