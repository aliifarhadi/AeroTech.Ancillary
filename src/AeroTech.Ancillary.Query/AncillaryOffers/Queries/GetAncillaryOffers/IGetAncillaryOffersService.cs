using AeroTech.Ancillary.Query.AncillaryOffers.Dto;
using AeroTech.Ancillary.Shopping.FlightOffers;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers
{
    public interface IGetAncillaryOffersService
    {
        Task<AncillaryOffersDto> ExecuteAsync(IAncillaryOffersQuery query, FlightOfferSalesScope scope, CancellationToken cancellationToken = default);
    }
}
