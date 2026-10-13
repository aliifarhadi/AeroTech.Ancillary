using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Api
{
    public sealed class ApiGetSeatOffersQueryHandler : IRequestHandler<ApiGetSeatOffersQuery, ApiSeatOffersDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOffersService _service;
        private readonly IApiOfferReferencesReader _references;

        public ApiGetSeatOffersQueryHandler(IApiSalesScopeResolver salesScope, IGetAncillaryOffersService service,
            IApiOfferReferencesReader references)
        {
            _salesScope = salesScope;
            _service = service;
            _references = references;
        }

        public async Task<ApiSeatOffersDto> Handle(ApiGetSeatOffersQuery query, CancellationToken cancellationToken)
        {
            var shopping = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), OfferFamilyFilters.Seat, cancellationToken);

            return ApiSeatOffersMapper.Map(shopping, await _references.ReadAsync(shopping, cancellationToken));
        }
    }
}
