using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Domain.Formatting
{
    public class LineMatches : ILineMatches
    {
        private readonly IObservable<IEnumerable<SearchMetadata>> _strings;

        public LineMatches(ISearchMetadataCollection searchMetadataCollection)
        {
            _strings = searchMetadataCollection.Metadata
                .Connect()
                .IgnoreUpdateWhen((current, previous) => SearchMetadata.EffectsHighlightComparer.Equals(current, previous))
                .QueryWhenChanged(query => query.Items.OrderBy(si => si.Position))
                .Replay(1)
                .RefCount();
        }

        public IObservable<LineMatchCollection> GetMatches(string inputText)
        {
            return _strings.Select(meta =>
            {
                //build list of matching filters
                return new LineMatchCollection(meta
                    .OrderBy(m => m.Position)
                    .Where(m => m.Predicate(inputText))
                   
                    .Select(m => new LineMatch(m))
                    .ToArray());
            }).StartWith(LineMatchCollection.Empty);
        }
    }
}