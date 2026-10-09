using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Projection;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.ActivateAirportSlotInventory
{
    public sealed class ActivateAirportSlotInventoryService : IActivateAirportSlotInventoryService
    {
        private readonly IAirportSlotInventoryRepository _inventories;
        private readonly IAirportSlotInventoryQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IAirportReference _airports;
        private readonly IAirportFacilityReference _facilities;
        private readonly IClock _clock;

        public ActivateAirportSlotInventoryService(
            IAirportSlotInventoryRepository inventories,
            IAirportSlotInventoryQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IInventoryCallerScope scope,
            IAirportReference airports,
            IAirportFacilityReference facilities,
            IClock clock)
        {
            _inventories = inventories;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _scope = scope;
            _airports = airports;
            _facilities = facilities;
            _clock = clock;
        }

        public async Task<AirportSlotInventoryResult> ActivateAsync(IActivateAirportSlotInventoryCommand command, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var inventory = await _inventories.GetAsync(command.InventoryId, cancellationToken);

            if (inventory is null || inventory.OwnerAirlineId != ownerAirlineId)
                throw ExceptionFactory.InventorySourceNotFound();

            var facility = await _facilities.CheckAsync(inventory.OwnerAirlineId, inventory.FacilityId, cancellationToken);

            inventory.Activate(
                new AirportSlotEvidence(
                    await _airports.ExistsAsync(inventory.AirportId, cancellationToken),
                    facility.Result,
                    facility.AirportId,
                    facility.TimeZoneId),
                command.ExpectedVersion,
                _clock.GetDateTime());

            await _synchronizer.ProjectAsync(inventory.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return inventory.ToResult();
        }
    }
}
