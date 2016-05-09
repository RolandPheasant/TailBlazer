using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public static class StartFromEx
    {
        public static IObservable<ILineProvider> StartFrom(this IObservable<ILineProvider> source, long startPosition)
        {            
            return source.Select(lineProvider => new StartFromLineProvider(lineProvider, startPosition));
        }

        public static IObservable<FileNotification> ScanFrom(this IObservable<FileNotification> source, long startFrom,
            TimeSpan? refreshPeriod = null,
            IScheduler scheduler = null)
        {
            return new FileRewriter(source, startFrom, refreshPeriod, scheduler).Notifications;
        }

        public static IObservable<FileNotification> ScanFromEnd(this IObservable<FileNotification> source,
            TimeSpan? refreshPeriod = null,
            IScheduler scheduler = null)
        {
             return new FileRewriter(source, refreshPeriod: refreshPeriod, scheduler: scheduler).Notifications;
        }
    }
}
