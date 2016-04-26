using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public static class StartFromEx
    {
        public static IObservable<ILineProvider> StartFrom(this IObservable<ILineProvider> source, long startPosition)
        {            
            return source.Select(lineProvider => new StartFromLineProvider(lineProvider, startPosition));
        }
    }
}
