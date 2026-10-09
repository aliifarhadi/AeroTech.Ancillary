using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision
{
    public sealed class ActivateAncillaryProvisionService : IActivateAncillaryProvisionService
    {
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly ISupplierRepository _suppliers;
        private readonly IAncillaryPricingRepository _pricings;
        private readonly IAncillaryProvisionQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public ActivateAncillaryProvisionService(
            IAncillaryProvisionRepository provisions,
            IAncillaryServiceDefinitionRepository definitions,
            ISupplierRepository suppliers,
            IAncillaryPricingRepository pricings,
            IAncillaryProvisionQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _provisions = provisions;
            _definitions = definitions;
            _suppliers = suppliers;
            _pricings = pricings;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<ProvisionResult> ActivateAsync(IActivateAncillaryProvisionCommand command, CancellationToken cancellationToken = default)
        {
            var provision = await _provisions.GetAsync(command.ProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.ProvisionNotFound();

            if (provision.Status != ProvisionStatus.Draft)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();

            var definition = await _definitions.GetAsync(provision.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ProvisionServiceDefinitionNotFound();

            if (definition.Status != ServiceDefinitionStatus.Active)
                throw ExceptionFactory.ProvisionServiceDefinitionNotActive();

            provision.EnsureAllowedBy(await _pricings.FindActiveAsync(provision.Id, cancellationToken));

            var now = _clock.GetDateTime();
            var superseded = await _provisions.FindActiveAtSequenceAsync(provision.ServiceDefinitionId, provision.Sequence, cancellationToken);

            if (superseded is not null && superseded.Id != provision.Id)
            {
                superseded.Supersede(now);
                await _synchronizer.ProjectAsync(superseded.ToReadModelSnapshot(), cancellationToken);
            }

            var supplier = await _suppliers.GetAsync(definition.SupplierId, cancellationToken);

            provision.Activate(definition, supplier?.QuoteAuthorityKey(), now);

            await _synchronizer.ProjectAsync(provision.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return provision.ToResult();
        }
    }
}
