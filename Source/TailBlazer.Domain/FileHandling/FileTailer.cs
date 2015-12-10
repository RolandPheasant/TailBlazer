using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{

    public sealed class Tailer : IEquatable<Tailer>, IDisposable
    {
        public string SearchText { get;  }
        public IObservable<ILineProvider> Latest { get;  }

     //   private readonly IDisposable _cleanUp;



        public Tailer([NotNull] string searchText, [NotNull] IObservable<ILineProvider> latest)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            SearchText = searchText;
            Latest = latest;
        }


        #region Equality

        public bool Equals(Tailer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(SearchText, other.SearchText);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Tailer)obj);
        }

        public override int GetHashCode()
        {
            return SearchText?.GetHashCode() ?? 0;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(Tailer left, Tailer right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Tailer left, Tailer right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{SearchText}";
        }
    }

    public interface ITailCollection : IDisposable
    {
        IObservableCache<Tailer, string> Tails { get; }

        void Add(string searchText, IObservable<ILineProvider> latest);
        void Remove(string searchText);
    }


    public sealed class TailCollection: ITailCollection
    {
        private readonly ISourceCache<Tailer, string> _searches = new SourceCache<Tailer, string>(t => t.SearchText);
        public IObservableCache<Tailer, string> Tails { get; }

        private readonly IDisposable _cleanUp;

        public TailCollection()
        {
            Tails = _searches.AsObservableCache();
            _cleanUp = new CompositeDisposable(_searches, Tails);
        }

        public void Add([NotNull] string searchText, [NotNull] IObservable<ILineProvider> latest)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            _searches.AddOrUpdate(new Tailer(searchText,latest));
        }

        public void Remove(string searchText)
        {
            _searches.Remove(searchText);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }

    public class FileTailer: IDisposable
    {
        private readonly IDisposable _cleanUp;
        public IObservable<int> TotalLines { get;  }
        public IObservable<int> MatchedLines { get; }
        public IObservable<long> FileSize { get; }
        public IObservableList<Line> Lines { get; }
        public IObservable<bool> IsLoading { get; }
        public IObservable<ILineProvider> Latest { get; }

        public IObservable<ILineProvider> All { get; }

        public FileTailer(FileInfo file,
                IObservable<FileSearchResult> filter,
                IObservable<ScrollRequest> scrollRequest,
                ILogger logger,
                IScheduler scheduler = null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Info($"Constructing file tailer for {file.FullName}");

            var lines = new SourceList<Line>();
            Lines = lines.AsObservableList();

            var locker = new object();
            scrollRequest = scrollRequest.Synchronize(locker); 
            
            var fileWatcher = file.WatchFile(scheduler: scheduler)
                            .DistinctUntilChanged()
                            .TakeWhile(notification => notification.Exists).Repeat()
                            .Replay(1).RefCount();

            var indexer = fileWatcher.Index().Replay(1).RefCount();
            
            //compare latest lines and latest filter and only take the filtered results it is not empty
            var latestLines = indexer.Cast<ILineProvider>().Synchronize(locker);
            var latestFilter = filter.Cast<ILineProvider>().Synchronize(locker); 
            var latest = latestLines.CombineLatest(latestFilter, (l, f) => f.IsEmpty ? l : f);

            All = latestLines;
            Latest = latest;
            MatchedLines = latest.Select(provider => provider.Count);
            TotalLines = latestLines.Select(x => x.Count);
            FileSize = fileWatcher.Select(notification => notification.Size);
            IsLoading = indexer.Take(1).Select(_ => false).StartWith(true);

            var aggregator = latest.CombineLatest(scrollRequest, (currentLines, scroll) => currentLines.ReadLines(scroll).ToArray())
                .Subscribe(currentPage =>
                {
                    var previous = lines.Items.ToArray();
                    var added = currentPage.Except(previous).ToArray();
                    var removed = previous.Except(currentPage).ToArray();

                    lines.Edit(innerList =>
                    {
                        if (removed.Any()) innerList.RemoveMany(removed);
                        if (added.Any()) innerList.AddRange(added);
                    });
                });

            _cleanUp = new CompositeDisposable(Lines, lines, aggregator);
        }



        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
