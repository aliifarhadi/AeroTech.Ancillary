using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Api
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
