using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ActivateInventoryPolicy
{
    public sealed class ActivateInventoryPolicyService : IActivateInventoryPolicyService
    {
        private readonly IInventoryPolicyRepository _policies;
        private readonly IInventoryPolicyQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IInventoryCommercialFactsReader _facts;
        private readonly IInventoryPolicyEvidenceBuilder _evidence;
        private readonly IClock _clock;

        public ActivateInventoryPolicyService(
            IInventoryPolicyRepository policies,
            IInventoryPolicyQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IInventoryCallerScope scope,
            IInventoryCommercialFactsReader facts,
            IInventoryPolicyEvidenceBuilder evidence,
            IClock clock)
        {
            _policies = policies;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _scope = scope;
            _facts = facts;
            _evidence = evidence;
            _clock = clock;
        }

        public async Task<InventoryPolicyResult> ActivateAsync(IActivateInventoryPolicyCommand command, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var policy = await _policies.GetAsync(command.PolicyId, cancellationToken);

            if (policy is null || policy.OwnerAirlineId != ownerAirlineId)
                throw ExceptionFactory.InventoryPolicyNotFound();

            var facts = await _facts.FindCurrentAsync(policy.OwnerAirlineId, policy.ServiceDefinitionRef, cancellationToken);

            if (facts?.Profile == AncillaryProfile.Seat)
                throw ExceptionFactory.InventoryPolicyNotApplicable(policy.ServiceDefinitionRef);

            policy.Activate(await _evidence.BuildAsync(policy, cancellationToken), command.ExpectedVersion, _clock.GetDateTime());

            await _synchronizer.ProjectAsync(policy.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return policy.ToResult();
        }
    }
}
