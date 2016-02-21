using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

// ReSharper disable once CheckNamespace
namespace DynamicData.Binding
{
    public static class DynamicDataEx
    {
        public static IObservable<IChangeSet<T>> RecordChanges<T>(this IObservable<IChangeSet<T>> source, ILogger logger,string label)
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

        public static IObservable<IChangeSet<TObject,TKey>> RecordChanges<TObject, TKey>(this IObservable<IChangeSet<TObject, TKey>> source, ILogger logger, string label)
        {
            if (!Debugger.IsAttached)
                return source;

            return source.Do(changes =>
            {
                logger.Info($"{label}: {changes.Count}");
            });
        }




        public static IObservable<IChangeSet<T>> ToObservableChangeSet<T>(this ReadOnlyObservableCollection<T> source)
        {
            return Observable.Create<IChangeSet<T>>
                (
                    observer =>
                    {
                        Func<ChangeSet<T>> initialChangeSet = () =>
                        {
                            var initial = new Change<T>(ListChangeReason.AddRange, source.ToList());
                            return new ChangeSet<T>() { initial };
                        };

                        //populate local cache, otherwise there is no way to deal with a reset
                        var cloneOfList = new SourceList<T>();

                        var sourceUpdates = Observable
                            .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                                h => ((INotifyCollectionChanged)source).CollectionChanged += h,
                                h => ((INotifyCollectionChanged)source).CollectionChanged -= h)
                            .Select
                            (
                                args =>
                                {
                                    var changes = args.EventArgs;

                                    switch (changes.Action)
                                    {
                                        case NotifyCollectionChangedAction.Add:
                                            return changes.NewItems.OfType<T>()
                                                .Select((t, index) => new Change<T>(ListChangeReason.Add, t, index + changes.NewStartingIndex));

                                        case NotifyCollectionChangedAction.Remove:
                                            return changes.OldItems.OfType<T>()
                                                .Select((t, index) => new Change<T>(ListChangeReason.Remove, t, index + changes.OldStartingIndex));

                                        case NotifyCollectionChangedAction.Replace:
                                        {
                                            return changes.NewItems.OfType<T>()
                                                .Select((t, idx) =>
                                                {
                                                    var old = changes.OldItems[idx];
                                                    return new Change<T>(ListChangeReason.Replace, t, (T)old, idx, idx);
                                                });
                                        }
                                        case NotifyCollectionChangedAction.Reset:
                                        {
                                            var cleared = new Change<T>(ListChangeReason.Clear, cloneOfList.Items.ToList(), 0);
                                            var clearedChangeSet = new ChangeSet<T>() { cleared };
                                            return clearedChangeSet.Concat(initialChangeSet());
                                        }

                                        case NotifyCollectionChangedAction.Move:
                                        {
                                            var item = changes.NewItems.OfType<T>().First();
                                            var change = new Change<T>(item, changes.NewStartingIndex, changes.OldStartingIndex);
                                            return new[] { change };
                                        }

                                        default:
                                            return null;
                                    }
                                })
                            .Where(updates => updates != null)
                            .Select(updates => (IChangeSet<T>)new ChangeSet<T>(updates));

                        var initialChanges = initialChangeSet();
                        var cacheLoader = Observable.Return(initialChanges).Concat(sourceUpdates).PopulateInto(cloneOfList);
                        var subscriber = cloneOfList.Connect().SubscribeSafe(observer);
                        return new CompositeDisposable(cacheLoader, subscriber, cloneOfList);
                    });
        }


    }
}