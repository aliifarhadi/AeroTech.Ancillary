namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IAirlineOfficeTimeZoneResolver
    {
        Task<string?> FindTimeZoneIdAsync(long airlineOfficeId, CancellationToken cancellationToken = default);
    }
}
