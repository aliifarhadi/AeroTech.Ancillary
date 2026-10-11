using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    public sealed class BackofficeGetAncillaryOffersQueryHandler : IRequestHandler<BackofficeGetAncillaryOffersQuery, BackofficeAncillaryOffersDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOffersService _service;

        public BackofficeGetAncillaryOffersQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeAncillaryOffersDto> Handle(BackofficeGetAncillaryOffersQuery query, CancellationToken cancellationToken)
            => BackofficeAncillaryOffersMapper.Map(
                await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(query.Options, cancellationToken), cancellationToken));
    }
}
