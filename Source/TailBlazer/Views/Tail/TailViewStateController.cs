using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.StateHandling;

namespace TailBlazer.Views.Tail
{
    public class TailViewStateController:IDisposable
    {
        private readonly TailViewModel _tailView;
        private readonly IDisposable _cleanUp;

        public TailViewStateController([NotNull] TailViewModel tailView, IStateBucketService stateBucketService, ISchedulerProvider schedulerProvider, ITailViewStateRestorer tailViewStateRestorer, ILogger logger, bool loadDefaults)
        {
            if (tailView == null) throw new ArgumentNullException(nameof(tailView));

            _tailView = tailView;


            var converter = new TailViewToStateConverter();

            bool loadingSettings=false;
            const string type = "TailView";
            logger.Info("Loading state for {0}", tailView.Name);

            //Load state

            if (loadDefaults)
            {
                stateBucketService
                    .Lookup(type, tailView.Name)
                    .IfHasValue(tailviewstate =>
                    {
                        schedulerProvider.Background.Schedule(() =>
                        {
                            try
                            {
                                loadingSettings = true;
                                tailViewStateRestorer.Restore(tailView, tailviewstate);
                            }
                            finally
                            {
                                loadingSettings = false;
                            }

                        });
                    });
            }
            
            //write latest to file when something triggers a staye change
            var selectedChanged = tailView.SearchCollection
                    .WhenValueChanged(sc=>sc.Selected,false)
                    .Select(vm=>vm.Text);

            var metaChanged = _tailView.SearchMetadataCollection.Metadata.Connect()
                    .ToCollection()
                    .Select(metaData => metaData.ToArray());

            var writer = selectedChanged.CombineLatest(metaChanged,(selected, metadata)=>new { selected , metadata })
                        .Where(_=> !loadingSettings)
                        .Throttle(TimeSpan.FromMilliseconds(250))
                        .Select(x => converter.Convert(_tailView.Name, x.selected, x.metadata))
                    .Subscribe(state =>
                    {
                        stateBucketService.Write(type, tailView.Name, state);
                    });

            _cleanUp = new CompositeDisposable(writer);
        }


        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}