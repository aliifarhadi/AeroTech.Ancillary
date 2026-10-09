using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy
{
    public sealed class ChangeInventoryPolicyService : IChangeInventoryPolicyService
    {
        private readonly IInventoryPolicyRepository _policies;
        private readonly IInventoryPolicyQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IInventoryCommercialFactsReader _facts;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public ChangeInventoryPolicyService(
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

        public async Task<InventoryPolicyResult> ChangeAsync(IChangeInventoryPolicyCommand command, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var policy = await _policies.GetAsync(command.PolicyId, cancellationToken);

            if (policy is null || policy.OwnerAirlineId != ownerAirlineId)
                throw ExceptionFactory.InventoryPolicyNotFound();

            var facts = await _facts.FindAsync(command.ServiceDefinitionId, cancellationToken);

            if (facts is null || facts.OwnerAirlineId != policy.OwnerAirlineId || facts.ServiceDefinitionRef != policy.ServiceDefinitionRef)
                throw ExceptionFactory.InventoryPolicyServiceDefinitionMismatch();

            policy.Change(
                InventoryPolicyInputMapper.ToArgs(
                command.ServiceDefinitionId,
                command.Authority,
                command.LocalPattern,
                command.ProviderKey,
                command.CountConsumption,
                command.WeightConsumption,
                command.SlotConsumption,
                command.PassengerUsageLimits),
                command.ExpectedVersion,
                _idGenerator,
                _clock.GetDateTime());

            await _synchronizer.ProjectAsync(policy.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return policy.ToResult();
        }
    }
}
