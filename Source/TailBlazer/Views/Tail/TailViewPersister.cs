using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using System.Xml.Linq;
using DynamicData;
using DynamicData.Kernel;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Settings;
using TailBlazer.Domain.StateHandling;
using TailBlazer.Views.Formatting;

namespace TailBlazer.Views.Tail
{
    public class TailViewStateController
    {
        private readonly ISearchMetadataCollection _searchMetadataCollection;
        private readonly IStateBucketService _stateBucketService;

        public TailViewStateController(TailViewModel tailView, 
            ISearchMetadataCollection searchMetadataCollection,
            IStateBucketService stateBucketService,
            IColourProvider colourProvider,
            IIconProvider iconProvider)
        {
            _searchMetadataCollection = searchMetadataCollection;
            _stateBucketService = stateBucketService;
        }


    }

    public interface ISearchStateToMetadataMapper
    {
        SearchMetadata Map(SearchState state);
    }

    public class SearchStateToMetadataMapper : ISearchStateToMetadataMapper
    {
        private readonly IColourProvider _colourProvider;
        private readonly IIconProvider _iconProvider;

        public SearchStateToMetadataMapper(IColourProvider colourProvider,
            IIconProvider iconProvider)
        {
            _colourProvider = colourProvider;
            _iconProvider = iconProvider;
        }

        public SearchMetadata Map(SearchState state)
        {
            var hue = _colourProvider
                        .Lookup(new HueKey(state.Swatch, state.Hue))
                        .ValueOr(()=> _colourProvider.DefaultHighlight);
                        

            return new SearchMetadata(state.Position, state.Text, state.Filter,state.Highlight, state.UseRegEx,state.IgnoreCase,
                hue,
                state.Icon);
        }
    }

    public class TailViewPersister: IPersistentStateProvider
    {
        private readonly ISearchMetadataCollection _searchMetadataCollection;
        private readonly TailViewModel _tailView;
        

        public TailViewPersister(TailViewModel tailView, 
            ISearchMetadataCollection searchMetadataCollection, 
            IStateBucketService stateBucketService,
            ISearchStateToMetadataMapper searchStateToMetadataMapper)
        {
            _searchMetadataCollection = searchMetadataCollection;
            _tailView = tailView;


            const string type = "TailView";

            stateBucketService
                .Lookup("TailView", tailView.Name)
                .Convert(state =>
                {
                    var converter = new TailViewToStateConverter();
                    return converter.Convert(state);
                }).IfHasValue(tailviewstate =>
                {
                    
                    //restore state
                    
                    tailviewstate.SearchItems.Select(searchStateToMetadataMapper.Map)
                    .ForEach(meta =>
                    {
                        searchMetadataCollection.AddorUpdate(meta);
                    });

                });

            var writer = searchMetadataCollection.Metadata
                .Connect()
                .ToCollection()
                .Select(x => Convert(x.ToArray()))
                .Subscribe(state =>
                {
                    stateBucketService.Write(type, tailView.Name, state);
                });


        }


        State IPersistentStateProvider.CaptureState()
        {
            return Convert(null);
        }

        public State Convert(SearchMetadata[] items=null)
        {
            var toWrite = items ?? _searchMetadataCollection.Metadata.Items;

            var searchItems = toWrite
                .OrderBy(s=>s.Position)
                .Select(search => new SearchState
                    (
                        search.SearchText,
                        search.Position,
                        search.UseRegex,
                        search.Highlight,
                        search.Filter,
                        false,
                        search.IgnoreCase,
                        search.HighlightHue.Swatch,
                           search.IconKind,
                        search.HighlightHue.Name
                     
                    )).ToArray();

            var tailViewState = new TailViewState(_tailView.Name,_tailView.SearchCollection.Selected.Text, searchItems);
            var converter = new TailViewToStateConverter();
            return converter.Convert(tailViewState);
        }

        private TailViewState RestoreState()
        {
            return null;
        }

        public void Restore(State state)
        {
            
        }
    }

    public sealed  class SearchState
    {
        public string Text { get; }
        public int Position { get; set; }
        public bool UseRegEx { get;  }
        public bool Highlight { get; }
        public bool Filter { get; }
        public bool Alert { get; }
        public bool IgnoreCase { get; }

        public string Swatch { get; }

        public string Hue { get; }

        public string Icon { get; }

        public SearchState(string text,int position, bool useRegEx, bool highlight, bool filter, bool alert, bool ignoreCase, string swatch, string icon, string hue)
        {
            Text = text;
            Position = position;
            UseRegEx = useRegEx;
            Highlight = highlight;
            Filter = filter;
            Alert = alert;
            IgnoreCase = ignoreCase;
            Swatch = swatch;
            Icon = icon;
            Hue = hue;
        }
    }

    public class TailViewToStateConverter : IConverter<TailViewState>
    {
        private static class Structure
        {
            public const string Root = "TailView";

            public const string FileName = "FileName";
            public const string SelectedFilter = "SelectedSearch";

            public const string SearchList = "SearchList";
            public const string SearchItem = "SearchItem";

            public const string Text = "Text";
            public const string Filter = "Filter";
            public const string UseRegEx = "UseRegEx";
            public const string Highlight = "Highlight";
            public const string Alert = "Alert";
            public const string IgnoreCase = "IgnoreCase";

            public const string Swatch = "Swatch";
            public const string Hue = "Hue";
            public const string Icon = "Icon";
        }

        public TailViewState Convert(State state)
        {
            if (state == null || state == State.Empty)
                return GetDefaultValue();

            var doc = XDocument.Parse(state.Value);

            var root = doc.ElementOrThrow(Structure.Root);
            var filename = root.ElementOrThrow(Structure.FileName);
            var selectedFilter = root.ElementOrThrow(Structure.SelectedFilter);

            var searchStates = root.Element(Structure.SearchList)
                            .Elements(Structure.SearchItem)
                            .Select((element,index) =>
                            {
                                var text = element.ElementOrThrow(Structure.Text);
                                var position = element.Attribute(Structure.Filter).Value.ParseInt().ValueOr(() => index);
                                var filter = element.Attribute(Structure.Filter).Value.ParseBool().ValueOr(() => true);
                                var useRegEx = element.Attribute(Structure.UseRegEx).Value.ParseBool().ValueOr(() => false);
                                var highlight = element.Attribute(Structure.Highlight).Value.ParseBool().ValueOr(() => true);
                                var alert = element.Attribute(Structure.Alert).Value.ParseBool().ValueOr(() => false);
                                var ignoreCase = element.Attribute(Structure.IgnoreCase).Value.ParseBool().ValueOr(() => true);

                                var swatch = element.Attribute(Structure.Swatch).Value;
                                var hue = element.Attribute(Structure.Hue).Value;
                                var icon = element.Attribute(Structure.Icon).Value;

                                return new SearchState(text, position, useRegEx,highlight,filter,alert,ignoreCase,swatch,icon,hue);
                            }).ToArray();
            return new TailViewState(filename, selectedFilter, searchStates);
        }

        public State Convert(TailViewState state)
        {
            if (state == null || state == TailViewState.Empty)
                return State.Empty;

            var root = new XElement(new XElement(Structure.Root));
            root.Add(new XElement(Structure.FileName, state.FileName));
            root.Add(new XElement(Structure.SelectedFilter, state.SelectedSearch));

            var list = new XElement(Structure.SearchList);

            var searchItemsArray = state.SearchItems.Select(f => new XElement(Structure.SearchItem,
                new XElement(Structure.Text, f.Text),
                new XAttribute(Structure.Filter, f.Filter),
                new XAttribute(Structure.UseRegEx, f.UseRegEx),
                new XAttribute(Structure.Highlight, f.Highlight),
                new XAttribute(Structure.Alert, f.Alert),
                new XAttribute(Structure.IgnoreCase, f.IgnoreCase),
                new XAttribute(Structure.Swatch, f.Swatch),
                new XAttribute(Structure.Hue, f.Hue),
                new XAttribute(Structure.Icon, f.Icon)));

            searchItemsArray.ForEach(list.Add);

            root.Add(list);

            XDocument doc = new XDocument(root);
            return new State(1, doc.ToString());
        }

        public TailViewState GetDefaultValue()
        {
            return TailViewState.Empty;
        }
    }
}
