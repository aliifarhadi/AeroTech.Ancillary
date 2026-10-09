using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Projection;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory
{
    public sealed class DefineAirportSlotInventoryService : IDefineAirportSlotInventoryService
    {
        private readonly IAirportSlotInventoryRepository _inventories;
        private readonly IAirportSlotInventoryQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IAirportReference _airports;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public DefineAirportSlotInventoryService(
            IAirportSlotInventoryRepository inventories,
            IAirportSlotInventoryQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IInventoryCallerScope scope,
            IAirportReference airports,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _inventories = inventories;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _scope = scope;
            _airports = airports;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<AirportSlotInventoryResult> DefineAsync(IDefineAirportSlotInventoryCommand command, CancellationToken cancellationToken = default)
        {
            await _scope.EnsureOwnerAsync(command.OwnerAirlineId, cancellationToken);

            if (!await _airports.ExistsAsync(command.AirportId, cancellationToken))
                throw ExceptionFactory.InventoryReferenceNotFound($"Airport {command.AirportId}");

            var inventory = AirportSlotInventory.Define(
                _idGenerator.NewId(),
                command.OwnerAirlineId,
                command.AirportId,
                command.FacilityId,
                command.StartUtc,
                command.EndUtc,
                command.CapacityPersons,
                _clock.GetDateTime());

            await using var facilityLock = await _inventories.LockFacilityAsync(command.OwnerAirlineId, command.FacilityId, cancellationToken);

            if (await _inventories.HasOverlapAsync(command.OwnerAirlineId, command.FacilityId, command.StartUtc, command.EndUtc, cancellationToken))
                throw ExceptionFactory.InventorySlotOverlap();

            await _inventories.AddAsync(inventory, cancellationToken);
            await _synchronizer.ProjectAsync(inventory.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return inventory.ToResult();
        }
    }
}
