using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.RetireInventoryPolicy
{
    public sealed class RetireInventoryPolicyService : IRetireInventoryPolicyService
    {
        private readonly IInventoryPolicyRepository _policies;
        private readonly IInventoryPolicyQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryCallerScope _scope;
        private readonly IClock _clock;

        public RetireInventoryPolicyService(
            IInventoryPolicyRepository policies,
            IInventoryPolicyQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IInventoryCallerScope scope,
            IClock clock)
        {
            _policies = policies;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _scope = scope;
            _clock = clock;
        }

        public async Task<InventoryPolicyResult> RetireAsync(IRetireInventoryPolicyCommand command, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var policy = await _policies.GetAsync(command.PolicyId, cancellationToken);

            if (policy is null || policy.OwnerAirlineId != ownerAirlineId)
                throw ExceptionFactory.InventoryPolicyNotFound();

            policy.Retire(command.ExpectedVersion, _clock.GetDateTime());

            await _synchronizer.ProjectAsync(policy.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return policy.ToResult();
        }
    }
}
