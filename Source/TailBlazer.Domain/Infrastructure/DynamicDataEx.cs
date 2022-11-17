using System.Diagnostics;
using System.Reactive.Linq;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

// ReSharper disable once CheckNamespace
namespace DynamicData.Binding;

public static class DynamicDataEx
{
    public static IObservable<IChangeSet<T>> LogChanges<T>(this IObservable<IChangeSet<T>> source, ILogger logger,string label)
    {
        if (!Debugger.IsAttached)
            return source;

        return source.Do(changes =>
        {
            logger.Info($"{label}: {changes.TotalChanges}");
        });
    }


    public static IObservable<T> LogErrors<T>(this IObservable<T> source, ILogger logger)
    {
        return source.Do(changes => { }, ex => logger.Error(ex, "There has been an error "));
    }

    public static IObservable<T> LogValueAndErrors<T>([NotNull] this IObservable<T> source, [NotNull] ILogger logger, [NotNull] Func<T, string> formatter)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        if (formatter == null) throw new ArgumentNullException(nameof(formatter));
        return source.Do(t =>  logger.Info(formatter(t)), ex => logger.Error(ex, "There has been an error "));
    }

    public static IObservable<IChangeSet<TObject,TKey>> LogChanges<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source, ILogger logger, string label)
    {
        if (!Debugger.IsAttached)
            return source;

        return source.Do(changes =>
        {
            logger.Info($"{label}: {changes.Count}");
        });
    }
 
}