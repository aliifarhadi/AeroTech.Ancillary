using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions.OtaPanel
{
    public sealed class OtaPanelGetSelectionOptionsQueryHandler : IRequestHandler<OtaPanelGetSelectionOptionsQuery, SelectionOptionsDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetSelectionOptionsService _service;

        public OtaPanelGetSelectionOptionsQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetSelectionOptionsService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<SelectionOptionsDto> Handle(OtaPanelGetSelectionOptionsQuery query, CancellationToken cancellationToken)
            => SelectionOptionsMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
