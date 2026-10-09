using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy
{
    public sealed class DefineInventoryPolicyService : IDefineInventoryPolicyService
    {
        private readonly IInventoryPolicyRepository _policies;
        private readonly IInventoryPolicyQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IInventoryCommercialFactsReader _facts;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public DefineInventoryPolicyService(
            IInventoryPolicyRepository policies,
            IInventoryPolicyQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IInventoryCallerScope scope,
            IInventoryCommercialFactsReader facts,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _policies = policies;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _scope = scope;
            _facts = facts;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<InventoryPolicyResult> DefineAsync(IDefineInventoryPolicyCommand command, CancellationToken cancellationToken = default)
        {
            await _scope.EnsureOwnerAsync(command.OwnerAirlineId, cancellationToken);

            var facts = await _facts.FindAsync(command.ServiceDefinitionId, cancellationToken);

            if (facts is null || facts.OwnerAirlineId != command.OwnerAirlineId || facts.ServiceDefinitionRef != command.ServiceDefinitionRef)
                throw ExceptionFactory.InventoryPolicyServiceDefinitionMismatch();

            if (await _policies.HasCurrentAsync(command.OwnerAirlineId, command.ServiceDefinitionRef, cancellationToken))
                throw ExceptionFactory.InventoryPolicyAlreadyExists(command.ServiceDefinitionRef);

            var policy = AncillaryInventoryPolicy.Define(
                _idGenerator.NewId(),
                command.OwnerAirlineId,
                command.ServiceDefinitionRef,
                InventoryPolicyInputMapper.ToArgs(
                command.ServiceDefinitionId,
                command.Authority,
                command.LocalPattern,
                command.ProviderKey,
                command.CountConsumption,
                command.WeightConsumption,
                command.SlotConsumption,
                command.PassengerUsageLimits),
                _idGenerator,
                _clock.GetDateTime());

            await _policies.AddAsync(policy, cancellationToken);
            await _synchronizer.ProjectAsync(policy.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return policy.ToResult();
        }
    }
}
