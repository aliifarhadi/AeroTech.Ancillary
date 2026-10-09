using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Projection;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory
{
    public sealed class AdjustFlightCountInventoryService : IAdjustFlightCountInventoryService
    {
        private readonly IFlightCountInventoryRepository _inventories;
        private readonly IFlightCountInventoryQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public AdjustFlightCountInventoryService(
            IFlightCountInventoryRepository inventories,
            IFlightCountInventoryQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IInventoryCallerScope scope,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _inventories = inventories;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _scope = scope;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<FlightCountAdjustmentResult> AdjustAsync(IAdjustFlightCountInventoryCommand command, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var inventory = await _inventories.GetAsync(command.InventoryId, cancellationToken);

            if (inventory is null || inventory.OwnerAirlineId != ownerAirlineId)
                throw ExceptionFactory.InventorySourceNotFound();

            var adjustment = inventory.AdjustTo(
                command.NewTotal,
                command.ReasonCode,
                _scope.RequireActorId(),
                command.CorrelationId,
                command.ExpectedVersion,
                _idGenerator,
                _clock.GetDateTime());

            await _synchronizer.ProjectAsync(inventory.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return adjustment.ToResult();
        }
    }
}
