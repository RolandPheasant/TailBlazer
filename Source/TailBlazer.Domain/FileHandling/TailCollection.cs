using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

using DynamicData;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class TailCollection : ITailCollection
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
            _searches.AddOrUpdate(new Tailer(searchText, latest));
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
}
