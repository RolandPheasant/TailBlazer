using System.Collections.Generic;

namespace TailBlazer.Views.Tail
{
    public sealed class TailViewState
    {
        public static readonly TailViewState Empty = new TailViewState();

        public IEnumerable<string> FilesName { get; }
        public string SelectedSearch { get; }

        public IEnumerable<SearchState> SearchItems { get; }

        public TailViewState(IEnumerable<string> filesName, string selectedSearch, IEnumerable<SearchState> searchItems)
        {
            FilesName = filesName;
            SelectedSearch = selectedSearch;
            SearchItems = searchItems;
        }

        private TailViewState()
        {
            
        }
    }
}