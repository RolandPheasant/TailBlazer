using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.Ratings;

public interface IRatingsMetricsLookup
{
    RatingsMetaData Lookup(int rating);
}