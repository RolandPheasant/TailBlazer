using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.Ratings;

public interface IRatingService
{
    IObservable<RatingsMetaData> Metrics { get; }
}