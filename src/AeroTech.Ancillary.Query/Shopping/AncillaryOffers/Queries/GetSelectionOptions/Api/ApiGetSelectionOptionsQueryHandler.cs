using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions.Api
{
    public sealed class ApiGetSelectionOptionsQueryHandler : IRequestHandler<ApiGetSelectionOptionsQuery, SelectionOptionsDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetSelectionOptionsService _service;

        public ApiGetSelectionOptionsQueryHandler(IApiSalesScopeResolver salesScope, IGetSelectionOptionsService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<SelectionOptionsDto> Handle(ApiGetSelectionOptionsQuery query, CancellationToken cancellationToken)
            => SelectionOptionsMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
