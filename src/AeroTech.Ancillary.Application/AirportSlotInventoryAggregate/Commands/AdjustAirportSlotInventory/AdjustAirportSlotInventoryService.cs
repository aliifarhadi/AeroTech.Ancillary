using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Projection;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.AdjustAirportSlotInventory
{
    public sealed class AdjustAirportSlotInventoryService : IAdjustAirportSlotInventoryService
    {
        private readonly IAirportSlotInventoryRepository _inventories;
        private readonly IInventoryLedger _ledger;
        private readonly IAirportSlotInventoryQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public AdjustAirportSlotInventoryService(
            IAirportSlotInventoryRepository inventories,
            IInventoryLedger ledger,
            IAirportSlotInventoryQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IInventoryCallerScope scope,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _inventories = inventories;
            _ledger = ledger;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _scope = scope;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<AirportSlotAdjustmentResult> AdjustAsync(IAdjustAirportSlotInventoryCommand command, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var source = new InventorySourceKey(InventoryResourceKind.AirportSlot, command.InventoryId);
            await using var ledger = await _ledger.OpenAsync([source], cancellationToken);
            var inventory = await _inventories.GetAsync(command.InventoryId, cancellationToken);

            if (inventory is null || inventory.OwnerAirlineId != ownerAirlineId)
                throw ExceptionFactory.InventorySourceNotFound();

            var activeUsage = (await ledger.BalancesAsync(_clock.GetDateTime(), cancellationToken))[source].ActiveUsage;
            var adjustment = inventory.AdjustTo(
                command.NewTotal,
                activeUsage,
                command.ReasonCode,
                _scope.RequireActorId(),
                command.CorrelationId,
                command.ExpectedVersion,
                _idGenerator,
                _clock.GetDateTime());

            await _synchronizer.ProjectAsync(inventory.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ledger.CommitAsync(cancellationToken);

            return adjustment.ToResult();
        }
    }
}
