using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Projection;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory
{
    public sealed class DefineFlightCountInventoryService : IDefineFlightCountInventoryService
    {
        private readonly IFlightCountInventoryRepository _inventories;
        private readonly IFlightCountInventoryQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public DefineFlightCountInventoryService(
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

        public async Task<FlightCountInventoryResult> DefineAsync(IDefineFlightCountInventoryCommand command, CancellationToken cancellationToken = default)
        {
            await _scope.EnsureOwnerAsync(command.OwnerAirlineId, cancellationToken);

            var unit = await _inventories.FindCountUnitOfLiveSourcesAsync(command.OwnerAirlineId, command.ResourceId, null, cancellationToken);

            if (unit is { } existing && existing != command.CountUnit)
                throw ExceptionFactory.InventoryUnitMismatch($"resource {command.ResourceId} is counted in {existing}");

            var inventory = FlightCountInventory.Define(
                _idGenerator.NewId(),
                command.OwnerAirlineId,
                command.FlightId,
                command.ResourceId,
                command.CountUnit,
                command.TotalCapacity,
                _clock.GetDateTime());

            await _inventories.AddAsync(inventory, cancellationToken);
            await _synchronizer.ProjectAsync(inventory.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return inventory.ToResult();
        }
    }
}
