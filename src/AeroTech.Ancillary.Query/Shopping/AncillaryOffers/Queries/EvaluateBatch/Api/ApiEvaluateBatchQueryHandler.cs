using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Api
{
    public sealed class ApiEvaluateBatchQueryHandler : IRequestHandler<ApiEvaluateBatchQuery, ApiSelectionBatchDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IEvaluateBatchService _service;
        private readonly IApiOfferReferencesReader _references;

        public ApiEvaluateBatchQueryHandler(IApiSalesScopeResolver salesScope, IEvaluateBatchService service, IApiOfferReferencesReader references)
        {
            _salesScope = salesScope;
            _service = service;
            _references = references;
        }

        public async Task<ApiSelectionBatchDto> Handle(ApiEvaluateBatchQuery query, CancellationToken cancellationToken)
        {
            var batch = await _service.ExecuteAsync(
                query,
                ApiSelectionLineInputMapper.ToRequests(query.Selections),
                await _salesScope.ResolveAsync(cancellationToken),
                cancellationToken);

            return ApiSelectionBatchMapper.Map(batch, await _references.ReadAsync(batch.Shopping, cancellationToken));
        }
    }
}
