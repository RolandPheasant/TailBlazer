using System;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public static class LineProviderEx
    {
        public static IObservable<AutoTailResponse> Tail(this IObservable<ILineProvider> source,
            [NotNull] IObservable<int> pageSize)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (pageSize == null) throw new ArgumentNullException(nameof(pageSize));

            return new AutoTail(source,pageSize).Tail();
        }

        public static IObservable<UserScrollResponse> Scroll(this IObservable<ILineProvider> source,
            [NotNull] IObservable<ScrollRequest> scrollRequest)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (scrollRequest == null) throw new ArgumentNullException(nameof(scrollRequest));

            return new UserScroll(source, scrollRequest).Scroll();
        }
    }
}