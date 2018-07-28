using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Domain.Formatting
{
    public class TextFormatter : ITextFormatter
    {
        private readonly IObservable<IEnumerable<SearchMetadata>> _strings;

        public TextFormatter(ICombinedSearchMetadataCollection searchMetadataCollection)
        {
            _strings = searchMetadataCollection.Combined
                .Connect(meta => (meta.Highlight == HighlightingMode.Text || meta.Highlight == HighlightingMode.Line) && !meta.IsExclusion)
                .IgnoreUpdateWhen((current, previous) => SearchMetadata.EffectsHighlightComparer.Equals(current, previous))
                .QueryWhenChanged(query => query.Items.OrderBy(m => m.Position))
                .Replay(1)
                .RefCount();
        }

        public IObservable<IEnumerable<DisplayText>> GetFormatter(string inputText)
        {
            return _strings.Select(meta =>
            {
                //split into 2 parts. 1) matching text 2) matching regex
                return inputText
                    .MatchString(meta)
                    .Select(ms => new DisplayText(ms))
                    .ToArray();
            });
        }
    }
}