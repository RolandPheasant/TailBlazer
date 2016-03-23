using System;

namespace Carnac.Logic.KeyMonitor
{
    public interface IInterceptKeys
    {
        IObservable<InterceptKeyArgs> GetKeyStream();
    }
}