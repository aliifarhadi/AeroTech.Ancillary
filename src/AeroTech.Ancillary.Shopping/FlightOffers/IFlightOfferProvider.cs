namespace AeroTech.Ancillary.Shopping.FlightOffers
{
    public interface IFlightOfferProvider
    {
        Task<FlightOffer> GetByOfferIdAsync(string offerId, CancellationToken cancellationToken = default);
    }
}
