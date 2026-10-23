using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions.Backoffice
{
    public sealed class BackofficeGetSelectionOptionsQueryHandler : IRequestHandler<BackofficeGetSelectionOptionsQuery, SelectionOptionsDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetSelectionOptionsService _service;

        public BackofficeGetSelectionOptionsQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetSelectionOptionsService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<SelectionOptionsDto> Handle(BackofficeGetSelectionOptionsQuery query, CancellationToken cancellationToken)
            => SelectionOptionsMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(null, cancellationToken), cancellationToken));
    }
}
