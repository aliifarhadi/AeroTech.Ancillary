namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IAirportReference
    {
        Task<bool> ExistsAsync(int airportId, CancellationToken cancellationToken = default);
    }
}
