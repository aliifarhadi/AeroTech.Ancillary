using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.Common.FlightOffers
{
    public interface IFlightOfferShoppingSource
    {
        Task<FlightOfferShopping> LoadAsync(string? offerId, AncillarySalesScope scope, CancellationToken cancellationToken = default);

        Task<FlightOfferShopping> ContextualizeAsync(FlightOffer offer, AncillarySalesScope scope, string sourceVersion, CancellationToken cancellationToken = default);
    }
}
