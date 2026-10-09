using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision
{
    public sealed class PublishAncillaryProvisionService : IPublishAncillaryProvisionService
    {
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly ICurrencyReference _currencies;
        private readonly IAncillaryPricingRepository _pricings;
        private readonly IAncillaryProvisionQueryDbSynchronizer _provisionSynchronizer;
        private readonly IAncillaryPricingQueryDbSynchronizer _pricingSynchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public PublishAncillaryProvisionService(
            IAncillaryProvisionRepository provisions,
            IAncillaryServiceDefinitionRepository definitions,
            ICurrencyReference currencies,
            IAncillaryPricingRepository pricings,
            IAncillaryProvisionQueryDbSynchronizer provisionSynchronizer,
            IAncillaryPricingQueryDbSynchronizer pricingSynchronizer,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _provisions = provisions;
            _definitions = definitions;
            _currencies = currencies;
            _pricings = pricings;
            _provisionSynchronizer = provisionSynchronizer;
            _pricingSynchronizer = pricingSynchronizer;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<ProvisionResult> PublishAsync(IPublishAncillaryProvisionCommand command, CancellationToken cancellationToken = default)
        {
            var provision = await _provisions.GetAsync(command.ProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.ProvisionNotFound();

            if (provision.Status != ProvisionStatus.Draft)
                throw ExceptionFactory.ProvisionStatusChangeNotAllowed();

            var pricing = await _pricings.GetAsync(command.PricingId, cancellationToken);

            if (pricing is null || pricing.AncillaryProvisionId != provision.Id)
                throw ExceptionFactory.PricingNotFound();

            var definition = await _definitions.GetAsync(provision.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ProvisionServiceDefinitionNotFound();

            if (definition.Status != ServiceDefinitionStatus.Active)
                throw ExceptionFactory.ProvisionServiceDefinitionNotActive();

            provision.EnsurePriceable();
            pricing.EnsureSameUnit(definition);

            var active = await _pricings.FindActiveAsync(provision.Id, cancellationToken);

            if (active is not null && active.Id != pricing.Id)
                throw ExceptionFactory.PricingAlreadyActive();

            var now = _clock.GetDateTime();

            if (active is null)
            {
                pricing.Activate(await _currencies.FindDecimalPlacesAsync(pricing, cancellationToken), now);
                await _pricingSynchronizer.ProjectAsync(pricing.ToReadModelSnapshot(), cancellationToken);
            }

            var superseded = await _provisions.FindActiveAtSequenceAsync(provision.ServiceDefinitionId, provision.Sequence, cancellationToken);

            if (superseded is not null && superseded.Id != provision.Id)
            {
                superseded.Supersede(now);
                await _provisionSynchronizer.ProjectAsync(superseded.ToReadModelSnapshot(), cancellationToken);
            }

            provision.Activate(definition, now);

            await _provisionSynchronizer.ProjectAsync(provision.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return provision.ToResult();
        }
    }
}
