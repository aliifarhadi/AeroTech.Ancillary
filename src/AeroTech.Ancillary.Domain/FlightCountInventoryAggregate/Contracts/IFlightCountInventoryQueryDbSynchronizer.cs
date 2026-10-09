using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts
{
    public interface IFlightCountInventoryQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(FlightCountInventoryReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
