using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts
{
    public interface IAirportSlotInventoryQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(AirportSlotInventoryReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
