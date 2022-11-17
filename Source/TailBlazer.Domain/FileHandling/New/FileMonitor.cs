using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using TailBlazer.Domain.Ratings;


namespace TailBlazer.Domain.FileHandling.New
{


    public class FileIndexer
    {
        /*
         1. Get length - this gives extent.
         2. Calculate average line length - gives position to scroll to
         3. Segment files??
         */

            /*
             1. Grab segments and load observable list
             2. [Clear segments when file is missing or size has reduced]
             3. From segments, we require index - use transform function
             4. Load details of last segment first
             5. Load latest page on tail - max 1 Mb [always keep in memory]

            */

        public FileIndexer()
        {
        }
    }

    public class FileMonitor
    {


        public  FileMonitor(string fullPath, IRatingService ratingsMetrics, IScheduler scheduler = null)
        {
            if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
            if (ratingsMetrics == null) throw new ArgumentNullException(nameof(ratingsMetrics));

            scheduler = scheduler ?? Scheduler.Default;

            var fileInfo = new FileInfo(fullPath);

            var refreshRate = ratingsMetrics.Metrics.Take(1)
                .Select(metrics => TimeSpan.FromMilliseconds(metrics.RefreshRate))
                .Wait();

           var fileChanged =  fileInfo
                .WatchFile(scheduler: scheduler, refreshPeriod: refreshRate);


        }
    }
}
