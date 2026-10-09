using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts
{
    public interface IFlightWeightInventoryQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(FlightWeightInventoryReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
