using AeroTech.Ancillary.Query.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    public sealed class BackofficeGetAncillaryOffersQueryHandler : IRequestHandler<BackofficeGetAncillaryOffersQuery, AncillaryOffersDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOffersService _service;

        public BackofficeGetAncillaryOffersQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<AncillaryOffersDto> Handle(BackofficeGetAncillaryOffersQuery query, CancellationToken cancellationToken)
            => await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(query.Options, cancellationToken), cancellationToken);
    }
}
