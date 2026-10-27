using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedOrderAncillaryOffers.Backoffice
{
    public sealed class BackofficeGetPricedOrderAncillaryOffersQueryHandler : IRequestHandler<BackofficeGetPricedOrderAncillaryOffersQuery, BackofficeOrderAncillaryOffersDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetPricedOrderAncillaryOffersService _service;

        public BackofficeGetPricedOrderAncillaryOffersQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetPricedOrderAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeOrderAncillaryOffersDto> Handle(BackofficeGetPricedOrderAncillaryOffersQuery query, CancellationToken cancellationToken)
            => BackofficeAncillaryOffersMapper.Map(await _service.ExecuteAsync(query.SelectionId, await _salesScope.ResolveAsync(null, cancellationToken), cancellationToken));
    }
}
