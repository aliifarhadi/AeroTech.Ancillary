using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
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
            => BackofficeAncillaryOffersMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(null, cancellationToken), cancellationToken));
    }
}
