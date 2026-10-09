namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IOperatorAirlineResolver
    {
        Task<long?> FindHomeAirlineIdAsync(CancellationToken cancellationToken = default);
    }
}
