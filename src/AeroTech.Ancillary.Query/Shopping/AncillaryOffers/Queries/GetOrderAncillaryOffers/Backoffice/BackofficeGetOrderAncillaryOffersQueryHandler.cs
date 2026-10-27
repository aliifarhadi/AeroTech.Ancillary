using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetOrderAncillaryOffers.Backoffice
{
    public sealed class BackofficeGetOrderAncillaryOffersQueryHandler : IRequestHandler<BackofficeGetOrderAncillaryOffersQuery, BackofficeOrderAncillaryOffersDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetOrderAncillaryOffersService _service;

        public BackofficeGetOrderAncillaryOffersQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetOrderAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeOrderAncillaryOffersDto> Handle(BackofficeGetOrderAncillaryOffersQuery query, CancellationToken cancellationToken)
            => BackofficeAncillaryOffersMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(null, cancellationToken), cancellationToken));
    }
}
