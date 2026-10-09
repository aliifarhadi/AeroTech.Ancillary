using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Projection;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.OpenFlightCountInventoryForSale
{
    public sealed class OpenFlightCountInventoryForSaleService : IOpenFlightCountInventoryForSaleService
    {
        private readonly IFlightCountInventoryRepository _inventories;
        private readonly IFlightCountInventoryQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IClock _clock;

        public OpenFlightCountInventoryForSaleService(
            IFlightCountInventoryRepository inventories,
            IFlightCountInventoryQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IInventoryCallerScope scope,
            IClock clock)
        {
            _inventories = inventories;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _scope = scope;
            _clock = clock;
        }

        public async Task<FlightCountInventoryResult> OpenAsync(IOpenFlightCountInventoryForSaleCommand command, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var inventory = await _inventories.GetAsync(command.InventoryId, cancellationToken);

            if (inventory is null || inventory.OwnerAirlineId != ownerAirlineId)
                throw ExceptionFactory.InventorySourceNotFound();

            inventory.OpenForSale(command.ExpectedVersion, _clock.GetDateTime());

            await _synchronizer.ProjectAsync(inventory.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return inventory.ToResult();
        }
    }
}
