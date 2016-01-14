using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Views.Formatting
{
    public class TextFormatter : ITextFormatter
    {
        private readonly IObservable<IEnumerable<SearchMetadata>> _strings;

        public TextFormatter(ISearchMetadataCollection searchMetadataCollection)
        {
            _strings = searchMetadataCollection.Metadata
                .Connect(meta => meta.Highlight)
                .IgnoreUpdateWhen((current, previous) => SearchMetadata.EffectsHighlightComparer.Equals(current, previous))
                .QueryWhenChanged(query => query.Items.Select(si => si))
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
                    .Select(ms => new DisplayText(ms));
            });
        }
    }
}