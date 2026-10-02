namespace AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts
{
    public interface IServiceSubCodeRepository
    {
        Task AddAsync(ServiceSubCode subCode, CancellationToken cancellationToken = default);

        Task<ServiceSubCode?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<ServiceSubCode?> FindAsync(int ownerAirlineId, string code, CancellationToken cancellationToken = default);

        Task<ServiceSubCode?> FindActiveAsync(int ownerAirlineId, string code, CancellationToken cancellationToken = default);
    }
}
