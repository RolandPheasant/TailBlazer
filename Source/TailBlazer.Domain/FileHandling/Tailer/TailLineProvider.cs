using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using TailBlazer.Domain.Infrastructure;


namespace TailBlazer.Domain.FileHandling.Tailer
{
    public class TailLineProvider
    {
        public TailLineProvider(IObservable<IndexCollection> lineProvider, IObservable<TailInfo> tailInfo)
        {
            //1. Use line provider until indexing is complete
            //2. Then start monitoring from the tail
            //3. Need something on IndexCollection to say scanning is complete




            var xxx = Observable.Create<IndexCollection>(observer =>
            {
                IndexCollection latest = null;



                var lineMonitor = lineProvider
                    .TakeWhile(lp => lp.IsComplete)
                    .WithContinuation(()=>tailInfo.Select(tail =>
                    {

                    }));


                return new CompositeDisposable();
            });

        }
    }
}
