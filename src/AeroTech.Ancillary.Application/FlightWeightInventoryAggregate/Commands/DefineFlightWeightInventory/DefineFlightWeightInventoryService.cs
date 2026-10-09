using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Projection;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory
{
    public sealed class DefineFlightWeightInventoryService : IDefineFlightWeightInventoryService
    {
        private readonly IFlightWeightInventoryRepository _inventories;
        private readonly IFlightWeightInventoryQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public DefineFlightWeightInventoryService(
            IFlightWeightInventoryRepository inventories,
            IFlightWeightInventoryQueryDbSynchronizer synchronizer,
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

        public async Task<FlightWeightInventoryResult> DefineAsync(IDefineFlightWeightInventoryCommand command, CancellationToken cancellationToken = default)
        {
            await _scope.EnsureOwnerAsync(command.OwnerAirlineId, cancellationToken);

            var inventory = FlightWeightInventory.Define(
                _idGenerator.NewId(),
                command.OwnerAirlineId,
                command.FlightId,
                command.WeightResourceId,
                command.CapacityKg,
                _clock.GetDateTime());

            await _inventories.AddAsync(inventory, cancellationToken);
            await _synchronizer.ProjectAsync(inventory.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return inventory.ToResult();
        }
    }
}
