using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.Common.FlightOffers
{
    public interface IFlightOfferShoppingSource
    {
        Task<FlightOfferShopping> LoadAsync(string? offerId, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
