using System.Reactive.Concurrency;
using System.Windows.Threading;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Infrastucture
{
    public class SchedulerProvider : ISchedulerProvider
    {
        public IScheduler MainThread { get; }

        public IScheduler Background { get; } = TaskPoolScheduler.Default;

        public SchedulerProvider(Dispatcher dispatcher)
        {
            MainThread = new DispatcherScheduler(dispatcher);
        }

    }
}