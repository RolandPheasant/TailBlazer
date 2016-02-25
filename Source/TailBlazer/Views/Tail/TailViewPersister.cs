using System;
using System.Linq;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Views.Tail
{
    public class TailViewPersister: IPersistentStateProvider
    {
        private readonly TailViewModel _tailView;
        private readonly ITailViewStateRestorer _tailViewStateRestorer;


        public TailViewPersister(TailViewModel tailView, ITailViewStateRestorer tailViewStateRestorer)
        {
            _tailView = tailView;
            _tailViewStateRestorer = tailViewStateRestorer;
        }


        State IPersistentStateProvider.CaptureState()
        {
            var coverter = new TailViewToStateConverter();
            var state = coverter.Convert(_tailView.Name, _tailView.SearchCollection.Selected.Text, _tailView.SearchMetadataCollection.Metadata.Items.ToArray());
            return new State(1, state.Value);
        }

        public void Restore(State state)
        {
            _tailViewStateRestorer.Restore(_tailView, state);
        }
    }
}
