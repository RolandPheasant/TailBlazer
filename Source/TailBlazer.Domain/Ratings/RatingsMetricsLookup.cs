using System.Collections.Generic;
using DynamicData.Kernel;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.Ratings
{
    public class RatingsMetricsLookup : IRatingsMetricsLookup
    {
       private IDictionary<int, RatingsMetaData> RatingMetrics { get; }

        public RatingsMetricsLookup()
        {

            RatingMetrics = new Dictionary<int, RatingsMetaData>
            {
                [1] = new RatingsMetaData(30, 1000),
                [2] = new RatingsMetaData(30, 750),
                [3] = new RatingsMetaData(45, 600),
                [4] = new RatingsMetaData(50, 450),
                [5] = new RatingsMetaData(60, 350)
            };
        }

       public RatingsMetaData Lookup(int rating)
       {
           return RatingMetrics.Lookup(rating)
               .ValueOr(() => RatingsMetaData.Default);
       }
    }
}
