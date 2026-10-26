using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Projection;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.RetireFlightWeightInventory
{
    public sealed class RetireFlightWeightInventoryService : IRetireFlightWeightInventoryService
    {
        private readonly IFlightWeightInventoryRepository _inventories;
        private readonly IInventoryLedger _ledger;
        private readonly IFlightWeightInventoryQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IClock _clock;

        public RetireFlightWeightInventoryService(
            IFlightWeightInventoryRepository inventories,
            IInventoryLedger ledger,
            IFlightWeightInventoryQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IInventoryCallerScope scope,
            IClock clock)
        {
            _inventories = inventories;
            _ledger = ledger;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _scope = scope;
            _clock = clock;
        }

        public async Task<FlightWeightInventoryResult> RetireAsync(IRetireFlightWeightInventoryCommand command, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var source = new InventorySourceKey(InventoryResourceKind.FlightWeight, command.InventoryId);
            await using var ledger = await _ledger.OpenAsync([source], cancellationToken);
            var inventory = await _inventories.GetAsync(command.InventoryId, cancellationToken);

            if (inventory is null || inventory.OwnerAirlineId != ownerAirlineId)
                throw ExceptionFactory.InventorySourceNotFound();

            var activeUsage = (await ledger.BalancesAsync(_clock.GetDateTime(), cancellationToken))[source].ActiveUsage;

            inventory.Retire(command.ExpectedVersion, activeUsage, _clock.GetDateTime());

            await _synchronizer.ProjectAsync(inventory.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ledger.CommitAsync(cancellationToken);

            return inventory.ToResult();
        }
    }
}
