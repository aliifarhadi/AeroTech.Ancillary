using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Backoffice
{
    public sealed class BackofficeGetSeatOffersQueryHandler : IRequestHandler<BackofficeGetSeatOffersQuery, BackofficeSeatOffersDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOffersService _service;

        public BackofficeGetSeatOffersQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeSeatOffersDto> Handle(BackofficeGetSeatOffersQuery query, CancellationToken cancellationToken)
        {
            var shopping = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(query.Options, cancellationToken), OfferFamilyFilters.Seat, cancellationToken);

            return BackofficeSeatOffersMapper.Map(shopping);
        }
    }
}
