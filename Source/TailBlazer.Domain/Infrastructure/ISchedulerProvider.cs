using System.Reactive.Concurrency;

namespace Trader.Domain.Infrastucture
{
    public interface ISchedulerProvider
    {
        IScheduler MainThread { get; }
        IScheduler TaskPool { get; }
    }
}
