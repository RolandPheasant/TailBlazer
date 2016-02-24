using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Domain.StateHandling;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class TailViewStateController:IDisposable
    {
        private readonly TailViewModel _tailView;

        private readonly IDisposable _cleanUp;

        public TailViewStateController([NotNull] TailViewModel tailView,
            IStateBucketService stateBucketService,
            ISchedulerProvider schedulerProvider, 
            ISearchStateToMetadataMapper searchStateToMetadataMapper, 
            ILogger logger)
        {
            if (tailView == null) throw new ArgumentNullException(nameof(tailView));

            _tailView = tailView;
            
            bool loadingSettings=false;
            const string type = "TailView";
            logger.Info("Loading state for {0}", tailView.Name);
            stateBucketService
                .Lookup(type, tailView.Name)
                .Convert(state =>
                {
                    var converter = new TailViewToStateConverter();
                    return converter.Convert((State) state);
                }).IfHasValue(tailviewstate =>
                {
                    schedulerProvider.Background.Schedule(() =>
                    {
                        try
                        {
                            loadingSettings = true;
                            logger.Info("Applying {0} saved search settings  for {1} ", tailviewstate.SearchItems.Count(), tailView.Name);
                            var searches = tailviewstate.SearchItems.Select(searchStateToMetadataMapper.Map);
                            _tailView.SearchMetadataCollection.Add(searches);
                            tailView.SearchCollection.Select(tailviewstate.SelectedSearch);
                            logger.Info("DONE: Applied {0} search settings for {1} ", tailviewstate.SearchItems.Count(), tailView.Name);
                        }
                        finally 
                        {
                            loadingSettings = false;
                        }

                    });
                });

            var selectedChanged = tailView.SearchCollection
                    .WhenValueChanged(sc=>sc.Selected,false)
                    .Select(vm=>vm.Text);

            var metaChanged = _tailView.SearchMetadataCollection.Metadata.Connect()
                    .ToCollection()
                    .Select(metaData => metaData.ToArray());

            var writer = selectedChanged.CombineLatest(metaChanged,(selected, metadata)=>new { selected , metadata })
                        .Where(_=> !loadingSettings)
                        .Throttle(TimeSpan.FromMilliseconds(250))
                        .Select(x => Convert(_tailView.Name, x.selected, x.metadata))
                    .Subscribe(state =>
                    {
                        stateBucketService.Write(type, tailView.Name, state);
                    });

            _cleanUp = new CompositeDisposable(writer);
        }

        public State Convert(string fileName, string selectedSearch, SearchMetadata[] items)
        {
      

            var searchItems = items
                .OrderBy(s => s.Position)
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

            var tailViewState = new TailViewState(fileName, selectedSearch, searchItems);
            var converter = new TailViewToStateConverter();
            return converter.Convert(tailViewState);
        }


        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}