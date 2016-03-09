using System;
using System.Reactive.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public static class FileSegmentEx
    {
        public static IObservable<FileSegmentCollection> WithSegments(this IObservable<FileNotification> source,int initialTail= 100000)
        {

            return source.Where(n=>(n.NotificationType == FileNotificationType.Changed || n.NotificationType == FileNotificationType.CreatedOrOpened))
                .Publish(shared =>
                {
                     return Observable.Create<FileSegmentCollection>(observer =>
                    {
                        var segmenter = new FileSegmenter(shared, initialTail);
                        return segmenter.Segments.SubscribeSafe(observer);
                    });
                });

        }

        public static IObservable<T> RepeatWhenFileShrinks<T>(this IObservable<FileSegmentCollection> source,
            Func<IObservable<FileSegmentCollection>, IObservable<T>> observableFactory)
        {
            return source
                .TakeWhile(fsc=>fsc.SizeDiff>0)
                .Publish(observableFactory)
                .Repeat();
        }
    }
}