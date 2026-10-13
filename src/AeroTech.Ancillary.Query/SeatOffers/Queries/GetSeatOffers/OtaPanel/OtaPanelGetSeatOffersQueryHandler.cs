using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.OtaPanel;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.OtaPanel
{
    public sealed class OtaPanelGetSeatOffersQueryHandler : IRequestHandler<OtaPanelGetSeatOffersQuery, OtaPanelSeatOffersDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOffersService _service;

        public OtaPanelGetSeatOffersQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OtaPanelSeatOffersDto> Handle(OtaPanelGetSeatOffersQuery query, CancellationToken cancellationToken)
        {
            var shopping = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), OfferFamilyFilters.Seat, cancellationToken);

            return OtaPanelSeatOffersMapper.Map(shopping);
        }
    }
}
