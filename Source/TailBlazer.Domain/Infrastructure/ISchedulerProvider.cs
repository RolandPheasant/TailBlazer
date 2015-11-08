using System.Reactive.Concurrency;

namespace TailBlazer.Domain.Infrastructure
{
    public interface ISchedulerProvider
    {
        IScheduler MainThread { get; }
        IScheduler TaskPool { get; }
    }
}
