using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Projection;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.ActivateFlightCountInventory
{
    public sealed class ActivateFlightCountInventoryService : IActivateFlightCountInventoryService
    {
        private readonly IFlightCountInventoryRepository _inventories;
        private readonly IFlightCountInventoryQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IFlightOccurrenceReference _flights;
        private readonly IInventoryResourceReference _resources;
        private readonly IClock _clock;

        public ActivateFlightCountInventoryService(
            IFlightCountInventoryRepository inventories,
            IFlightCountInventoryQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IInventoryCallerScope scope,
            IFlightOccurrenceReference flights,
            IInventoryResourceReference resources,
            IClock clock)
        {
            _inventories = inventories;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _scope = scope;
            _flights = flights;
            _resources = resources;
            _clock = clock;
        }

        public async Task<FlightCountInventoryResult> ActivateAsync(IActivateFlightCountInventoryCommand command, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var inventory = await _inventories.GetAsync(command.InventoryId, cancellationToken);

            if (inventory is null || inventory.OwnerAirlineId != ownerAirlineId)
                throw ExceptionFactory.InventorySourceNotFound();

            inventory.Activate(
                new FlightSourceEvidence(
                    await _flights.CheckAsync(inventory.OwnerAirlineId, inventory.FlightId, cancellationToken),
                    await _resources.CheckAsync(inventory.OwnerAirlineId, InventoryResourceKind.FlightCount, inventory.ResourceId, cancellationToken),
                    await _inventories.FindCountUnitOfLiveSourcesAsync(inventory.OwnerAirlineId, inventory.ResourceId, inventory.Id, cancellationToken)),
                command.ExpectedVersion,
                _clock.GetDateTime());

            await _synchronizer.ProjectAsync(inventory.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return inventory.ToResult();
        }
    }
}
