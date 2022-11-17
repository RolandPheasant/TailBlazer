using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail;

public class TailViewStateRestorer : ITailViewStateRestorer
{
    private readonly ILogger _logger;
    private readonly ISearchStateToMetadataMapper _searchStateToMetadataMapper;

    public TailViewStateRestorer(ILogger logger, ISearchStateToMetadataMapper searchStateToMetadataMapper)
    {
        _logger = logger;
        _searchStateToMetadataMapper = searchStateToMetadataMapper;
    }

    public void Restore(TailViewModel view, State state)
    {
        var converter = new TailViewToStateConverter();
        Restore(view,converter.Convert(state));
    }

    public void Restore(TailViewModel view,TailViewState tailviewstate)
    {
        _logger.Info("Applying {0} saved search settings  for {1} ", tailviewstate.SearchItems.Count(), view.Name);
        var searches = tailviewstate.SearchItems.Select(state=>_searchStateToMetadataMapper.Map(state,false));
        view.SearchMetadataCollection.Add(searches);
        view.SearchCollection.Select(tailviewstate.SelectedSearch);
        _logger.Info("DONE: Applied {0} search settings for {1} ", tailviewstate.SearchItems.Count(), view.Name);
    }
}