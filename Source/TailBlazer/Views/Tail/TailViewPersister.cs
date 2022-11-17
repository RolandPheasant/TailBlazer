using TailBlazer.Domain.Settings;

namespace TailBlazer.Views.Tail;

public class TailViewPersister: IPersistentView
{
    private readonly TailViewModel _tailView;
    private readonly ITailViewStateRestorer _tailViewStateRestorer;

    public TailViewPersister(TailViewModel tailView, ITailViewStateRestorer tailViewStateRestorer)
    {
        _tailView = tailView;
        _tailViewStateRestorer = tailViewStateRestorer;
    }
        
    ViewState IPersistentView.CaptureState()
    {
        var coverter = new TailViewToStateConverter();
        var state = coverter.Convert(_tailView.Name, _tailView.SearchCollection.Selected.Text, _tailView.SearchMetadataCollection.Metadata.Items.ToArray());
        return new ViewState(TailViewModelConstants.ViewKey, state);
    }

    public void Restore(ViewState state)
    {
        _tailViewStateRestorer.Restore(_tailView, state.State);
    }
}