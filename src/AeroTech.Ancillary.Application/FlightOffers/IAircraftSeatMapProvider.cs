namespace AeroTech.Ancillary.Application.FlightOffers
{
    public interface IAircraftSeatMapProvider
    {
        Task<AircraftSeatMap> GetAsync(int aircraftId, CancellationToken cancellationToken = default);
    }
}
