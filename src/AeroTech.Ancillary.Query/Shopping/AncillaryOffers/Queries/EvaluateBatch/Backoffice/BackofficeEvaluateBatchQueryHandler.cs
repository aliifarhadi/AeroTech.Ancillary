using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Backoffice
{
    public sealed class BackofficeEvaluateBatchQueryHandler : IRequestHandler<BackofficeEvaluateBatchQuery, BackofficeSelectionBatchDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IEvaluateBatchService _service;

        public BackofficeEvaluateBatchQueryHandler(IBackofficeSalesScopeResolver salesScope, IEvaluateBatchService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeSelectionBatchDto> Handle(BackofficeEvaluateBatchQuery query, CancellationToken cancellationToken)
            => BackofficeSelectionBatchMapper.Map(
                await _service.ExecuteAsync(
                    query,
                    BackofficeSelectionLineInputMapper.ToRequests(query.Selections),
                    await _salesScope.ResolveAsync(query.Options, cancellationToken),
                    cancellationToken));
    }
}
