using System;
using System.Linq;
using System.Reactive.Linq;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{


    public class AutoTail: IAutoTail
    {
        private readonly IObservable<ILineProvider> _lineProvider;
        private readonly IObservable<int> _pageSize;

        public AutoTail([NotNull] IObservable<ILineProvider> lineProvider, [NotNull] IObservable<int> pageSize )
        {
            if (lineProvider == null) throw new ArgumentNullException(nameof(lineProvider));
            if (pageSize == null) throw new ArgumentNullException(nameof(pageSize));
            _lineProvider = lineProvider;
            _pageSize = pageSize;
        }

        public IObservable<AutoTailResponse> Tail()
        {
           return _lineProvider.CombineLatest(_pageSize, (lp, pge) => new { LineProvider = lp, PageSize = pge })
                    .Scan(AutoTailResponse.Empty, (last, latest) =>
                    {
                        if (last == AutoTailResponse.Empty && last.PageSize == latest.PageSize && last.TailInfo.Start == latest.LineProvider.TailInfo.Start)
                            return last;

                        AutoTailReason reason;
                        if (latest.PageSize != last.PageSize)
                        {
                            reason = AutoTailReason.LoadTail;
                            var result = latest.LineProvider.ReadLines(new ScrollRequest(latest.PageSize)).ToArray();
                            return new AutoTailResponse(new TailInfo(result), latest.PageSize, reason);
                        }
                        reason = AutoTailReason.NewLines;
                        return new AutoTailResponse(latest.LineProvider.TailInfo.Trim(latest.PageSize), latest.PageSize, reason);
                    })
                    .DistinctUntilChanged()
                    .Where(result => result.Lines.Length != 0)
                    .Select(info => info);
        }
    }
}