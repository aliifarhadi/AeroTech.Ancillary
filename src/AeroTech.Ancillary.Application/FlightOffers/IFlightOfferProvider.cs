namespace AeroTech.Ancillary.Application.FlightOffers
{
    public interface IFlightOfferProvider
    {
        Task<FlightOffer> GetByOfferIdAsync(string offerId, CancellationToken cancellationToken = default);
    }
}
