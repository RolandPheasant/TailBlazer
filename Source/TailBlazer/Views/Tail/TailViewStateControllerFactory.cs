using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.StateHandling;

namespace TailBlazer.Views.Tail
{
    public class TailViewStateControllerFactory : ITailViewStateControllerFactory
    {
        private readonly IStateBucketService _stateBucketService;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly ITailViewStateRestorer _tailViewStateRestorer;
        private readonly ILogFactory _loggerFactory;

        public TailViewStateControllerFactory([NotNull] IStateBucketService stateBucketService,
            [NotNull] ISchedulerProvider schedulerProvider, 
            [NotNull] ITailViewStateRestorer tailViewStateRestorer, 
            [NotNull] ILogFactory loggerFactory)
        {
            if (stateBucketService == null) throw new ArgumentNullException(nameof(stateBucketService));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (tailViewStateRestorer == null) throw new ArgumentNullException(nameof(tailViewStateRestorer));
      
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _stateBucketService = stateBucketService;
            _schedulerProvider = schedulerProvider;
            _tailViewStateRestorer = tailViewStateRestorer;
            _loggerFactory = loggerFactory;
        }

        public IDisposable Create(TailViewModel tailView, bool loadDefaults)
        {
            if (tailView == null) throw new ArgumentNullException(nameof(tailView));

            var logger = _loggerFactory.Create<TailViewStateController>();
            return new TailViewStateController(tailView,_stateBucketService, _schedulerProvider, _tailViewStateRestorer, logger, loadDefaults);
        }
    }
}