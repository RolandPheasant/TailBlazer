using System;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.StateHandling;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Tail
{
    public class TailViewStateControllerFactory : ITailViewStateControllerFactory
    {
        private readonly IStateBucketService _stateBucketService;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly ISearchStateToMetadataMapper _searchStateToMetadataMapper;
        private readonly ILogFactory _loggerFactory;

        public TailViewStateControllerFactory([NotNull] IStateBucketService stateBucketService,
            [NotNull] ISchedulerProvider schedulerProvider,
            [NotNull] ISearchStateToMetadataMapper searchStateToMetadataMapper, 
            [NotNull] ILogFactory loggerFactory)
        {
            if (stateBucketService == null) throw new ArgumentNullException(nameof(stateBucketService));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (searchStateToMetadataMapper == null)
                throw new ArgumentNullException(nameof(searchStateToMetadataMapper));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _stateBucketService = stateBucketService;
            _schedulerProvider = schedulerProvider;
            _searchStateToMetadataMapper = searchStateToMetadataMapper;
            _loggerFactory = loggerFactory;
        }

        public IDisposable Create([NotNull] TailViewModel tailView)
        {
            if (tailView == null) throw new ArgumentNullException(nameof(tailView));

            var logger = _loggerFactory.Create<TailViewStateController>();
            return new TailViewStateController(tailView,_stateBucketService, _schedulerProvider,_searchStateToMetadataMapper, logger);
        }
    }
}