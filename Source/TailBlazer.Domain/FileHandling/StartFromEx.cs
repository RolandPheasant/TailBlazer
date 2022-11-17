using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace TailBlazer.Domain.FileHandling;

public static class StartFromEx
{
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

        return Observable.Create<FileNotification>(observer =>
        {
            return new FileRewriter(source, refreshPeriod: refreshPeriod, scheduler: scheduler)
                .Notifications
                .SubscribeSafe(observer);
        }).Replay(1).RefCount();
    }
}