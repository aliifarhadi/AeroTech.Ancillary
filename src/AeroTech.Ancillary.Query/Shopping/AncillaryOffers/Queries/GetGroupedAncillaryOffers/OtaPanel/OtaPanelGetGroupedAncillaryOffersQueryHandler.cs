using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelGetGroupedAncillaryOffersQueryHandler : IRequestHandler<OtaPanelGetGroupedAncillaryOffersQuery, OtaPanelGroupedAncillaryOffersDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetGroupedAncillaryOffersService _service;

        public OtaPanelGetGroupedAncillaryOffersQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetGroupedAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OtaPanelGroupedAncillaryOffersDto> Handle(OtaPanelGetGroupedAncillaryOffersQuery query, CancellationToken cancellationToken)
            => OtaPanelGroupedAncillaryOffersMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
