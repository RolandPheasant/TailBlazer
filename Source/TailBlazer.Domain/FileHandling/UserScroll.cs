using System;
using System.Linq;
using System.Reactive.Linq;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class UserScroll : IUserScroll
    {
        private readonly IObservable<ILineProvider> _lineProvider;
        private readonly IObservable<ScrollRequest> _scrollRequest;

        public UserScroll([NotNull] IObservable<ILineProvider> lineProvider, [NotNull] IObservable<ScrollRequest> scrollRequest)
        {
            if (lineProvider == null) throw new ArgumentNullException(nameof(lineProvider));
            if (scrollRequest == null) throw new ArgumentNullException(nameof(scrollRequest));
            _lineProvider = lineProvider;
            _scrollRequest = scrollRequest;
        }

        public IObservable<UserScrollResponse> Scroll()
        {
            return _lineProvider.CombineLatest(_scrollRequest, (lp, request) => new { LineProvider = lp, Request = request })
                    .Scan(UserScrollResponse.Empty, (last, latest) =>
                    {
                        var request = latest.Request;

                        //only scroll if it comes from a new user request
                        if (last.PageSize == request.PageSize 
                                    && last.FirstIndex == request.FirstIndex)
                            return last;

                        var result = latest.LineProvider.ReadLines(request).ToArray();
                        return new UserScrollResponse(latest.LineProvider.TailInfo, request.PageSize, request.FirstIndex, result);
                    })
                    .DistinctUntilChanged()
                    .Where(result => result.Lines.Length != 0)
                    .Select(info => info);
        }
    }
}