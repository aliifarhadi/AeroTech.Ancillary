using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.Backoffice
{
    public sealed class BackofficeGetSeatOfferDetailQueryHandler : IRequestHandler<BackofficeGetSeatOfferDetailQuery, BackofficeSeatOfferDetailDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOfferDetailService _service;

        public BackofficeGetSeatOfferDetailQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetAncillaryOfferDetailService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeSeatOfferDetailDto> Handle(BackofficeGetSeatOfferDetailQuery query, CancellationToken cancellationToken)
        {
            var detail = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(query.Options, cancellationToken), OfferFamilyFilters.Seat, cancellationToken);

            return BackofficeSeatOffersMapper.Detail(detail);
        }
    }
}
