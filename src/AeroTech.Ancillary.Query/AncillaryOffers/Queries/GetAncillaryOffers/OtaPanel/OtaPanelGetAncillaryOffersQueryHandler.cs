using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelGetAncillaryOffersQueryHandler : IRequestHandler<OtaPanelGetAncillaryOffersQuery, OtaPanelAncillaryOffersDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOffersService _service;

        public OtaPanelGetAncillaryOffersQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OtaPanelAncillaryOffersDto> Handle(OtaPanelGetAncillaryOffersQuery query, CancellationToken cancellationToken)
            => OtaPanelAncillaryOffersMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), OfferFamilyFilters.Services, cancellationToken));
    }
}
