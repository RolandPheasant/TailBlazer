using System.Reactive.Linq;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.Ratings;

public class RatingService: IRatingService
{
    public IObservable<RatingsMetaData> Metrics { get; }

    public RatingService(ISetting<GeneralOptions> setting, IRatingsMetricsLookup ratingMetricsLookup)
    {
        Metrics = setting.Value.Select(options => options.Rating)
            .DistinctUntilChanged()
            .Select(ratingMetricsLookup.Lookup);
    }
}