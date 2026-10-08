using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision
{
    public sealed class ReactivateAncillaryProvisionService : IReactivateAncillaryProvisionService
    {
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly IAncillaryPricingRepository _pricings;
        private readonly IAncillaryProvisionQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public ReactivateAncillaryProvisionService(
            IAncillaryProvisionRepository provisions,
            IAncillaryServiceDefinitionRepository definitions,
            IAncillaryPricingRepository pricings,
            IAncillaryProvisionQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork)
        {
            _provisions = provisions;
            _definitions = definitions;
            _pricings = pricings;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProvisionResult> ReactivateAsync(IReactivateAncillaryProvisionCommand command, CancellationToken cancellationToken = default)
        {
            var provision = await _provisions.GetAsync(command.ProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.ProvisionNotFound();
            var definition = await _definitions.GetAsync(provision.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ProvisionServiceDefinitionNotFound();

            provision.Reactivate(definition);
            provision.EnsureAllowedBy(await _pricings.FindActiveAsync(provision.Id, cancellationToken));

            var active = await _provisions.FindActiveAtSequenceAsync(provision.ServiceDefinitionId, provision.Sequence, cancellationToken);

            if (active is not null && active.Id != provision.Id)
                throw ExceptionFactory.ProvisionSequenceAlreadyActive(provision.Sequence);

            await _synchronizer.ProjectAsync(provision.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return provision.ToResult();
        }
    }
}
