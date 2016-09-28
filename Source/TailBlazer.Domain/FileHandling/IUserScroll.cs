using System;
using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling
{
    public interface IUserScroll
    {
        IObservable<UserScrollResponse> Scroll();
    }
}