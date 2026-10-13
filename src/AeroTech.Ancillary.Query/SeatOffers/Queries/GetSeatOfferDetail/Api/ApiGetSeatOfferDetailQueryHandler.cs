using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Api;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.Api
{
    public sealed class ApiGetSeatOfferDetailQueryHandler : IRequestHandler<ApiGetSeatOfferDetailQuery, ApiSeatOfferDetailDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOfferDetailService _service;
        private readonly IApiOfferReferencesReader _references;

        public ApiGetSeatOfferDetailQueryHandler(IApiSalesScopeResolver salesScope, IGetAncillaryOfferDetailService service,
            IApiOfferReferencesReader references)
        {
            _salesScope = salesScope;
            _service = service;
            _references = references;
        }

        public async Task<ApiSeatOfferDetailDto> Handle(ApiGetSeatOfferDetailQuery query, CancellationToken cancellationToken)
        {
            var detail = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), OfferFamilyFilters.Seat, cancellationToken);

            return ApiSeatOffersMapper.Detail(detail, await _references.ReadAsync(detail.Shopping, cancellationToken));
        }
    }
}
