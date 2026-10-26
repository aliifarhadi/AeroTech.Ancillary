using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.Backoffice
{
    public sealed class BackofficeGetPricedAncillaryOffersQueryHandler : IRequestHandler<BackofficeGetPricedAncillaryOffersQuery, BackofficeAncillaryOffersDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetPricedAncillaryOffersService _service;

        public BackofficeGetPricedAncillaryOffersQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetPricedAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeAncillaryOffersDto> Handle(BackofficeGetPricedAncillaryOffersQuery query, CancellationToken cancellationToken)
            => BackofficeAncillaryOffersMapper.Map(await _service.ExecuteAsync(query.SelectionId, await _salesScope.ResolveAsync(null, cancellationToken), cancellationToken));
    }
}
