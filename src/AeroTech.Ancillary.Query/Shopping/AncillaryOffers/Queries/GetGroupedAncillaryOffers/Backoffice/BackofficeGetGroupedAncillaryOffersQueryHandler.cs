using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers.Backoffice
{
    public sealed class BackofficeGetGroupedAncillaryOffersQueryHandler : IRequestHandler<BackofficeGetGroupedAncillaryOffersQuery, BackofficeGroupedAncillaryOffersDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetGroupedAncillaryOffersService _service;

        public BackofficeGetGroupedAncillaryOffersQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetGroupedAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeGroupedAncillaryOffersDto> Handle(BackofficeGetGroupedAncillaryOffersQuery query, CancellationToken cancellationToken)
            => BackofficeGroupedAncillaryOffersMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(query.Options, cancellationToken), cancellationToken));
    }
}
