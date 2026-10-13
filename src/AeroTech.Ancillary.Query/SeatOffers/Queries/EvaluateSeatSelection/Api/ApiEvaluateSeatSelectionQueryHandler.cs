using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Api;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.Api
{
    public sealed class ApiEvaluateSeatSelectionQueryHandler : IRequestHandler<ApiEvaluateSeatSelectionQuery, ApiSeatSelectionResultDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IEvaluateSelectionService _service;
        private readonly IApiOfferReferencesReader _references;

        public ApiEvaluateSeatSelectionQueryHandler(IApiSalesScopeResolver salesScope, IEvaluateSelectionService service,
            IApiOfferReferencesReader references)
        {
            _salesScope = salesScope;
            _service = service;
            _references = references;
        }

        public async Task<ApiSeatSelectionResultDto> Handle(ApiEvaluateSeatSelectionQuery query, CancellationToken cancellationToken)
        {
            var selection = await _service.ExecuteAsync(
                query,
                ApiSelectionInputMapper.ToSelection(query.Selection),
                await _salesScope.ResolveAsync(cancellationToken),
                OfferFamilyFilters.Seat,
                cancellationToken);

            return ApiSeatOffersMapper.Selection(selection, await _references.ReadAsync(selection.Shopping, cancellationToken));
        }
    }
}
