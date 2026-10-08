using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Projection;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ActivateAncillaryPricing
{
    public sealed class ActivateAncillaryPricingService : IActivateAncillaryPricingService
    {
        private readonly IAncillaryPricingRepository _pricings;
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly IAncillaryPricingQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public ActivateAncillaryPricingService(
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

        public async Task<PricingResult> ActivateAsync(IActivateAncillaryPricingCommand command, CancellationToken cancellationToken = default)
        {
            var pricing = await _pricings.GetAsync(command.PricingId, cancellationToken)
                          ?? throw ExceptionFactory.PricingNotFound();
            var provision = await _provisions.GetAsync(pricing.AncillaryProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.PricingProvisionNotFound();
            var definition = await _definitions.GetAsync(provision.ServiceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ProvisionServiceDefinitionNotFound();

            provision.EnsurePriceable();
            pricing.EnsureSameUnit(definition);

            if (await _pricings.FindActiveAsync(provision.Id, cancellationToken) is not null)
                throw ExceptionFactory.PricingAlreadyActive();

            pricing.Activate(_clock.GetDateTime());

            await _synchronizer.ProjectAsync(pricing.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return pricing.ToResult();
        }
    }
}
