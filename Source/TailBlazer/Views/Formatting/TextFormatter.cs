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
        private readonly IObservable<IEnumerable<string>> _strings;

        public TextFormatter(ISearchMetadataCollection searchMetadataCollection)
        {
            _strings = searchMetadataCollection.Metadata
                .Connect(meta => meta.Highlight)
                .IgnoreUpdateWhen((current, previous) => current.Highlight == previous.Highlight)
                .QueryWhenChanged(query => query.Items.Select(si => si.SearchText))
                .Replay(1)
                .RefCount();
        }

        public IObservable<IEnumerable<DisplayText>> GetFormatter(string inputText)
        {
            return _strings.Select(searchText =>
            {
                return inputText
                    .MatchString(searchText)
                    .Select(ms => new DisplayText(ms));
            });
        }
    }
}