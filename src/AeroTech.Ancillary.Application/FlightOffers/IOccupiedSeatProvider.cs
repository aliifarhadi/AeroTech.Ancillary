namespace AeroTech.Ancillary.Application.FlightOffers
{
    public interface IOccupiedSeatProvider
    {
        Task<IReadOnlyDictionary<long, IReadOnlySet<string>>> GetAsync(IReadOnlyCollection<long> flightIds, CancellationToken cancellationToken = default);
    }
}
