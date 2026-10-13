using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Api
{
    public sealed class ApiEvaluateSelectionQueryHandler : IRequestHandler<ApiEvaluateSelectionQuery, ApiSelectionResultDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IEvaluateSelectionService _service;
        private readonly IApiOfferReferencesReader _references;

        public ApiEvaluateSelectionQueryHandler(
            IApiSalesScopeResolver salesScope,
            IEvaluateSelectionService service,
            IApiOfferReferencesReader references)
        {
            _salesScope = salesScope;
            _service = service;
            _references = references;
        }

        public async Task<ApiSelectionResultDto> Handle(ApiEvaluateSelectionQuery query, CancellationToken cancellationToken)
        {
            var selection = await _service.ExecuteAsync(
                query,
                ApiSelectionInputMapper.ToSelection(query.Selection),
                await _salesScope.ResolveAsync(cancellationToken),
                OfferFamilyFilters.Services,
                cancellationToken);

            return ApiSelectionResultMapper.Map(selection, await _references.ReadAsync(selection.Shopping, cancellationToken));
        }
    }
}
