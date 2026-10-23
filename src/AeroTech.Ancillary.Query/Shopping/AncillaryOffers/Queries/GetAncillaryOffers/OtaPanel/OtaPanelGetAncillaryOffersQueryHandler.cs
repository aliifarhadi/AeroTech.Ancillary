using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelGetAncillaryOffersQueryHandler : IRequestHandler<OtaPanelGetAncillaryOffersQuery, AncillaryOffersDto>
    {
        private const string Surface = "OtaPanel";

        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOffersService _service;

        public OtaPanelGetAncillaryOffersQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<AncillaryOffersDto> Handle(OtaPanelGetAncillaryOffersQuery query, CancellationToken cancellationToken)
            => AncillaryOffersMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken), Surface);
    }
}
