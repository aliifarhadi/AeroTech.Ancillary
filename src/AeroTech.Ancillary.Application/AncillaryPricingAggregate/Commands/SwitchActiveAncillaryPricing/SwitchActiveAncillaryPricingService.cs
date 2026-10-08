using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing
{
    public sealed class SwitchActiveAncillaryPricingService : ISwitchActiveAncillaryPricingService
    {
        private readonly IAncillaryPricingRepository _pricings;
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly IAncillaryPricingQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public SwitchActiveAncillaryPricingService(
            IAncillaryPricingRepository pricings,
            IAncillaryProvisionRepository provisions,
            IAncillaryServiceDefinitionRepository definitions,
            IAncillaryPricingQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _pricings = pricings;
            _provisions = provisions;
            _definitions = definitions;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<PricingResult> SwitchAsync(ISwitchActiveAncillaryPricingCommand command, CancellationToken cancellationToken = default)
        {
            var provision = await _provisions.GetAsync(command.ProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.ProvisionNotFound();
            var pricing = await _pricings.GetAsync(command.NewPricingId, cancellationToken);

            if (pricing is null || pricing.AncillaryProvisionId != provision.Id)
                throw ExceptionFactory.PricingNotFound();

            var definition = await _definitions.GetAsync(provision.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ProvisionServiceDefinitionNotFound();

            provision.EnsurePriceable();
            pricing.EnsureSameUnit(definition);

            var active = await _pricings.FindActiveAsync(provision.Id, cancellationToken);

            if (command.ExpectedOldPricingId is { } expected && expected != active?.Id)
                throw ExceptionFactory.PricingSwitchExpectationFailed();

            if (active is not null && active.Id == pricing.Id)
                throw ExceptionFactory.PricingAlreadyActive();

            var now = _clock.GetDateTime();

            if (active is not null)
            {
                active.Supersede(now);
                await _synchronizer.ProjectAsync(active.ToReadModelSnapshot(), cancellationToken);
            }

            if (pricing.Status == PricingStatus.Suspended)
                pricing.Reactivate();
            else
                pricing.Activate(now);

            await _synchronizer.ProjectAsync(pricing.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return pricing.ToResult();
        }
    }
}
