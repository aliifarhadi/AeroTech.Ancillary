using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap.Backoffice
{
    public sealed class BackofficeGetSeatMapQueryHandler : IRequestHandler<BackofficeGetSeatMapQuery>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetSeatMapService _service;

        public BackofficeGetSeatMapQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetSeatMapService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task Handle(BackofficeGetSeatMapQuery query, CancellationToken cancellationToken)
            => await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(query.Options, cancellationToken), cancellationToken);
    }
}
