using System;
using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public interface IAutoTail
    {
        IObservable<AutoTailResponse> Tail();
    }
}